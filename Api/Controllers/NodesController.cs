using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Contracts.Configurations;
using Api.Exceptions;
using Api.Requests;
using Api.Responses;
using Contracts.Enums;
using Contracts.Values;
using Infrastructure.Services;
using Infrastructure.States;
using Infrastructure.Values;
using RemoteNode.Exceptions;
using RemoteNode.Models;
using RemoteNode.Services;
using Repositories.Entities;

namespace Api.Controllers;

/// <summary>
/// Manages remote XRayne nodes.
/// </summary>
[Authorize(Policy = AdminPermissionNames.SuperAdmin)]
[Authorize(Policy = AdminPermissionNames.ChangeXraySettings)]
[Route("api/nodes")]
public sealed class NodesController(
    IMapper mapper,
    INodeService nodes,
    INodeSecretService secrets,
    INodeConnectionVerifier connectionVerifier,
    IRemoteNodeConnectionManager connectionManager,
    IRemoteNodeApiClientFactory apiClientFactory,
    IBackgroundTaskScheduler scheduler,
    INodeProvisionStateMachine provisionStates,
    IEventStreamManager eventStreams,
    IHostEnvironment environment,
    IOptions<NodeConnectionOptions> nodeConnectionOptions) : ApiControllerBase
{
    private readonly GitHubReleaseClient xrayRepository = new(CoreDefaults.XrayRepositoryUrl);

    /// <summary>
    /// Gets all remote nodes available to administrators with node permissions.
    /// </summary>
    [HttpGet]
    [EndpointSummary("List nodes")]
    [EndpointDescription("Get all remote nodes available to administrators with node permissions.")]
    [ProducesResponseType(typeof(List<NodeDto>), StatusCodes.Status200OK)]
    public async Task<List<NodeDto>> GetAll(CancellationToken cancellationToken)
    {
        var result = await nodes.GetAllAsync(cancellationToken);

        return mapper.Map<List<NodeDto>>(result);
    }

    /// <summary>
    /// Gets a remote node by id.
    /// </summary>
    [HttpGet("{id:long}")]
    [EndpointSummary("Get node")]
    [EndpointDescription("Get a remote node by id.")]
    [ProducesResponseType(typeof(NodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NodeDto> GetById(long id, CancellationToken cancellationToken)
    {
        var node = await GetAccessibleNodeAsync(id, cancellationToken);

        return mapper.Map<NodeDto>(node);
    }

    /// <summary>
    /// Gets current remote node system status.
    /// </summary>
    [HttpGet("{id:long}/system/status")]
    [EndpointSummary("Remote node system status")]
    [EndpointDescription("Get system status and host telemetry from a remote node.")]
    [ProducesResponseType(typeof(SystemStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<SystemStatusResponse> GetSystemStatus(long id, CancellationToken cancellationToken)
    {
        var node = await GetAccessibleNodeAsync(id, cancellationToken);

        try
        {
            return await CreateRemoteNodeClient(node).GetSystemStatusAsync(cancellationToken);
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Creates a remote node and schedules provisioning.
    /// </summary>
    [HttpPost]
    [EndpointSummary("Create node")]
    [EndpointDescription("Create a remote node record and schedule SSH provisioning.")]
    [ProducesResponseType(typeof(CreateNodeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateNodeRequest request,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            ValidateCreateRequest(request);
        }

        var apiKey = GetCreateApiKey();
        var address = NormalizeAddress(request.Address);
        var certificateMode = System.Net.IPAddress.TryParse(address, out _)
            ? CertificateMode.Ip
            : CertificateMode.Domain;

        var node = new NodeEntity
        {
            Name = request.Name.Trim(),
            Address = address,
            Port = request.Port,
            ApiPort = request.ApiPort,
            SSHUsername = request.SSHUsername.Trim(),
            AuthType = request.AuthType,
            SSHKey = string.IsNullOrWhiteSpace(request.SSHKey) ? null : request.SSHKey,
            Password = string.IsNullOrWhiteSpace(request.Password) ? null : request.Password,
            WorkingDirectory = request.WorkingDirectory.Trim(),
            Note = request.Note.Trim(),
            EncryptedApiKey = secrets.ProtectApiKey(apiKey),
            ApiKeyFingerprint = secrets.GetFingerprint(apiKey),
            CertificateMode = certificateMode,
            Status = NodeStatus.Connecting,
            LastStatusChange = DateTime.UtcNow,
            InstallationMessage = "Remote node provisioning is queued."
        };

        await nodes.AddAsync(AdminId, node, cancellationToken);

        if (environment.IsDevelopment())
        {
            var devJobId = $"dev-{Guid.NewGuid():N}";
            await ConnectDevelopmentNodeAsync(node, apiKey, devJobId, cancellationToken);
            await connectionManager.EnsureConnectedAsync(node.Id, cancellationToken);

            return Created($"/api/nodes/{node.Id}", new CreateNodeResponse(mapper.Map<NodeDto>(node), devJobId));
        }

        var jobId = await scheduler.ScheduleProvisionNode(node.Id, cancellationToken);
        provisionStates.Dispatch(jobId, NodeProvisionState.Queued(node.Id, jobId));

        return Created($"/api/nodes/{node.Id}", new CreateNodeResponse(mapper.Map<NodeDto>(node), jobId));
    }

    /// <summary>
    /// Gets current remote node xray-core status.
    /// </summary>
    [HttpGet("{id:long}/core/status")]
    [EndpointSummary("Remote node core status")]
    [EndpointDescription("Get the current xray-core status from a remote node.")]
    [ProducesResponseType(typeof(CoreStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<CoreStatusResponse> GetCoreStatus(long id, CancellationToken cancellationToken)
    {
        var node = await GetAccessibleNodeAsync(id, cancellationToken);

        try
        {
            return await CreateRemoteNodeClient(node).GetCoreStatusAsync(cancellationToken);
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Streams current remote node xray-core status.
    /// </summary>
    [HttpGet("{id:long}/core/status/stream")]
    [EndpointSummary("Remote node core status stream")]
    [EndpointDescription("Subscribe to remote node xray-core status changes.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task StreamCoreStatus(long id, CancellationToken cancellationToken)
    {
        var node = await GetAccessibleNodeAsync(id, cancellationToken);
        var client = CreateRemoteNodeClient(node);

        SetupStreamHeaders();

        try
        {
            await Response.StartAsync(cancellationToken);
            await foreach (var state in client.CoreStatusStreamAsync(cancellationToken))
            {
                await WriteServerSentEventAsync(state, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (RemoteNodeException exception)
        {
            if (!Response.HasStarted)
            {
                throw ToApiException(exception);
            }
        }
    }

    /// <summary>
    /// Gets available xray-core releases for remote node installation.
    /// </summary>
    [HttpGet("{id:long}/core/releases")]
    [EndpointSummary("Remote node Xray releases")]
    [EndpointDescription("Get available xray-core releases for remote node installation.")]
    [ProducesResponseType(typeof(List<GitHubReleaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<List<GitHubReleaseDto>> GetCoreReleases(
        long id,
        [FromQuery] CoreReleasesQuery query,
        CancellationToken cancellationToken)
    {
        _ = await GetAccessibleNodeAsync(id, cancellationToken);
        var releases = await xrayRepository.GetReleasesAsync(query.PerPage, query.Page, cancellationToken);

        return releases.Select(mapper.Map<GitHubReleaseDto>).ToList();
    }

    /// <summary>
    /// Schedules remote node xray-core installation or reinstallation.
    /// </summary>
    [HttpPost("{id:long}/core/install")]
    [EndpointSummary("Install remote node Xray")]
    [EndpointDescription("Install or reinstall xray-core on a remote node.")]
    [ProducesResponseType(typeof(InstallCoreResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InstallCoreResponse>> InstallCore(
        long id,
        [FromBody] RemoteNode.Models.InstallCoreRequest request,
        CancellationToken cancellationToken)
    {
        var node = await GetAccessibleNodeAsync(id, cancellationToken);

        try
        {
            return Accepted(await CreateRemoteNodeClient(node).InstallCoreAsync(request, cancellationToken));
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Gets remote node xray-core installation status.
    /// </summary>
    [HttpGet("{id:long}/core/install/{jobId}/status")]
    [EndpointSummary("Remote node Xray install status")]
    [EndpointDescription("Get xray-core installation status from a remote node.")]
    [ProducesResponseType(typeof(InstallCoreStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<InstallCoreStatusResponse> GetInstallCoreStatus(
        long id,
        string jobId,
        CancellationToken cancellationToken)
    {
        var node = await GetAccessibleNodeAsync(id, cancellationToken);

        try
        {
            return await CreateRemoteNodeClient(node).GetInstallCoreStatusAsync(jobId, cancellationToken);
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
    }

    /// <summary>
    /// Streams remote node xray-core installation status.
    /// </summary>
    [HttpGet("{id:long}/core/install/{jobId}/stream")]
    [EndpointSummary("Remote node Xray install stream")]
    [EndpointDescription("Subscribe to xray-core installation status from a remote node.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task StreamInstallCoreStatus(
        long id,
        string jobId,
        CancellationToken cancellationToken)
    {
        var node = await GetAccessibleNodeAsync(id, cancellationToken);
        var client = CreateRemoteNodeClient(node);

        SetupStreamHeaders();

        try
        {
            await Response.StartAsync(cancellationToken);
            await foreach (var state in client.InstallCoreStatusStreamAsync(jobId, cancellationToken))
            {
                await WriteServerSentEventAsync(state, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (RemoteNodeException exception)
        {
            if (!Response.HasStarted)
            {
                throw ToApiException(exception);
            }
        }
    }

    /// <summary>
    /// Schedules remote node xray-core start.
    /// </summary>
    [HttpPost("{id:long}/core/start")]
    [EndpointSummary("Start remote node Xray")]
    [EndpointDescription("Start xray-core on a remote node.")]
    [ProducesResponseType(typeof(OperationAcceptedResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<ActionResult<OperationAcceptedResponse>> StartCore(long id, CancellationToken cancellationToken)
        => ScheduleCoreOperation(id, client => client.StartCoreAsync(cancellationToken), cancellationToken);

    /// <summary>
    /// Schedules remote node xray-core stop.
    /// </summary>
    [HttpPost("{id:long}/core/stop")]
    [EndpointSummary("Stop remote node Xray")]
    [EndpointDescription("Stop xray-core on a remote node.")]
    [ProducesResponseType(typeof(OperationAcceptedResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<ActionResult<OperationAcceptedResponse>> StopCore(long id, CancellationToken cancellationToken)
        => ScheduleCoreOperation(id, client => client.StopCoreAsync(cancellationToken), cancellationToken);

    /// <summary>
    /// Schedules remote node xray-core restart.
    /// </summary>
    [HttpPost("{id:long}/core/restart")]
    [EndpointSummary("Restart remote node Xray")]
    [EndpointDescription("Restart xray-core on a remote node.")]
    [ProducesResponseType(typeof(OperationAcceptedResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<ActionResult<OperationAcceptedResponse>> RestartCore(long id, CancellationToken cancellationToken)
        => ScheduleCoreOperation(id, client => client.RestartCoreAsync(cancellationToken), cancellationToken);

    /// <summary>
    /// Streams remote node provisioning state.
    /// </summary>
    [HttpGet("{id:long}/install/{jobId}/stream")]
    [EndpointSummary("Node provisioning stream")]
    [EndpointDescription("Subscribe to remote node provisioning state changes.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task StreamProvisionState(long id, string jobId, CancellationToken cancellationToken)
    {
        _ = await GetAccessibleNodeAsync(id, cancellationToken);
        var currentState = provisionStates.GetState(jobId);
        if (currentState is null || currentState.NodeId != id)
        {
            throw new NotFoundException($"Provisioning job '{jobId}' was not found.");
        }

        var subscription = eventStreams.Subscribe<NodeProvisionState>(NodeProvisionStateMachine.GetStreamKey(jobId));

        SetupStreamHeaders();

        try
        {
            await Response.StartAsync(cancellationToken);
            await WriteServerSentEventAsync(currentState, cancellationToken);

            await foreach (var state in subscription.Reader.ReadAllAsync(cancellationToken))
            {
                await WriteServerSentEventAsync(state, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        finally
        {
            eventStreams.Unsubscribe(subscription.Id);
        }
    }

    /// <summary>
    /// Manually resets reconnect attempts and schedules node reconnect.
    /// </summary>
    [HttpPost("{id:long}/reconnect")]
    [EndpointSummary("Reconnect node")]
    [EndpointDescription("Reset failed reconnect attempts and schedule a manual remote node reconnect.")]
    [ProducesResponseType(typeof(NodeOperationResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reconnect(long id, CancellationToken cancellationToken)
    {
        var node = await GetAccessibleNodeAsync(id, cancellationToken);
        node.Status = NodeStatus.Connecting;
        node.ReconnectAttemptCount = 0;
        node.Message = "Manual reconnect requested.";
        node.LastStatusChange = DateTime.UtcNow;

        await nodes.UpdateAsync(node, cancellationToken);
        await connectionManager.ReconnectAsync(node.Id, cancellationToken);

        return Accepted(new NodeOperationResponse(mapper.Map<NodeDto>(node), "reconnect_queued"));
    }

    /// <summary>
    /// Disables a remote node.
    /// </summary>
    [HttpPost("{id:long}/disable")]
    [EndpointSummary("Disable node")]
    [EndpointDescription("Disable a remote node and stop automatic reconnect attempts.")]
    [ProducesResponseType(typeof(NodeOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NodeOperationResponse> Disable(long id, CancellationToken cancellationToken)
    {
        var node = await GetAccessibleNodeAsync(id, cancellationToken);
        node.Status = NodeStatus.Disabled;
        node.Message = "Node disabled by user.";
        node.LastStatusChange = DateTime.UtcNow;
        await nodes.UpdateAsync(node, cancellationToken);
        await connectionManager.DisconnectAsync(node.Id, cancellationToken);

        return new NodeOperationResponse(mapper.Map<NodeDto>(node), "disabled");
    }

    /// <summary>
    /// Enables a disabled remote node.
    /// </summary>
    [HttpPost("{id:long}/enable")]
    [EndpointSummary("Enable node")]
    [EndpointDescription("Enable a disabled remote node and allow reconnect attempts.")]
    [ProducesResponseType(typeof(NodeOperationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<NodeOperationResponse> Enable(long id, CancellationToken cancellationToken)
    {
        var node = await GetAccessibleNodeAsync(id, cancellationToken);
        node.Status = NodeStatus.Connecting;
        node.Message = "Node enabled by user.";
        node.LastStatusChange = DateTime.UtcNow;
        await nodes.UpdateAsync(node, cancellationToken);
        await connectionManager.EnsureConnectedAsync(node.Id, cancellationToken);

        return new NodeOperationResponse(mapper.Map<NodeDto>(node), "enabled");
    }

    /// <summary>
    /// Deletes a remote node.
    /// </summary>
    [HttpDelete("{id:long}")]
    [EndpointSummary("Delete node")]
    [EndpointDescription("Delete a remote node and its saved connection configuration.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var deleted = await nodes.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Node '{id}' was not found.");
        }

        await connectionManager.DisconnectAsync(id, cancellationToken);

        return NoContent();
    }

    private async Task<NodeEntity> GetAccessibleNodeAsync(long id, CancellationToken cancellationToken)
    {
        var node = await nodes.GetByIdAsync(id, cancellationToken);

        return node ?? throw new NotFoundException($"Node '{id}' was not found.");
    }

    private static void ValidateCreateRequest(CreateNodeRequest request)
    {
        if (request.AuthType == SSHAuthType.Password && string.IsNullOrWhiteSpace(request.Password))
        {
            throw new BadRequestException("SSH password is required for password authentication.");
        }

        if (request.AuthType == SSHAuthType.PrivateKey && string.IsNullOrWhiteSpace(request.SSHKey))
        {
            throw new BadRequestException("SSH private key is required for private key authentication.");
        }
    }

    private static string NormalizeAddress(string value)
    {
        var address = value.Trim().TrimEnd('.').ToLowerInvariant();
        if (address.Length == 0)
        {
            throw new BadRequestException("Node address is required.");
        }

        return address;
    }

    private string GetCreateApiKey()
    {
        if (environment.IsDevelopment()
            && !string.IsNullOrWhiteSpace(nodeConnectionOptions.Value.DevelopmentApiKey))
        {
            return nodeConnectionOptions.Value.DevelopmentApiKey;
        }

        return secrets.GenerateApiKey();
    }

    private IRemoteNodeApiClient CreateRemoteNodeClient(NodeEntity node)
    {
        var endpoint = new RemoteNodeEndpoint(
            node.Id,
            node.Address,
            node.ApiPort,
            secrets.UnprotectApiKey(node.EncryptedApiKey));

        return apiClientFactory.Create(endpoint);
    }

    private async Task<ActionResult<OperationAcceptedResponse>> ScheduleCoreOperation(
        long id,
        Func<IRemoteNodeApiClient, Task<OperationAcceptedResponse>> operation,
        CancellationToken cancellationToken)
    {
        var node = await GetAccessibleNodeAsync(id, cancellationToken);

        try
        {
            return Accepted(await operation(CreateRemoteNodeClient(node)));
        }
        catch (RemoteNodeException exception)
        {
            throw ToApiException(exception);
        }
    }

    private static ApiException ToApiException(RemoteNodeException exception)
    {
        return exception switch
        {
            RemoteNodeHttpException httpException when httpException.ResponseBody is not null
                => new BadRequestException($"{httpException.Message} {httpException.ResponseBody}"),
            _ => new BadRequestException(exception.Message)
        };
    }

    private async Task ConnectDevelopmentNodeAsync(
        NodeEntity node,
        string apiKey,
        string jobId,
        CancellationToken cancellationToken)
    {
        provisionStates.Dispatch(jobId, NodeProvisionState.Queued(node.Id, jobId));
        provisionStates.Dispatch(jobId, NodeProvisionState.Preparing(node.Id, jobId));
        provisionStates.Dispatch(jobId, NodeProvisionState.Verifying(node.Id, jobId));

        try
        {
            var result = await connectionVerifier.VerifyAsync(node, apiKey, cancellationToken);

            node.Status = NodeStatus.Connected;
            node.ConnectedAt = result.VerifiedAt;
            node.LastSeenAt = result.VerifiedAt;
            node.XrayVersion = result.XrayVersion;
            node.ReconnectAttemptCount = 0;
            node.InstallationMessage = "Development SSH provisioning skipped. Local node is connected.";
            node.Message = null;
            node.LastStatusChange = DateTime.UtcNow;

            await nodes.UpdateAsync(node, cancellationToken);
            provisionStates.Dispatch(jobId, NodeProvisionState.Completed(node.Id, jobId));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            node.Status = NodeStatus.Error;
            node.Message = exception.Message;
            node.InstallationMessage = exception.Message;
            node.LastStatusChange = DateTime.UtcNow;

            await nodes.UpdateAsync(node, cancellationToken);
            provisionStates.Dispatch(jobId, NodeProvisionState.Failed(node.Id, jobId, exception.Message));
        }
    }
}
