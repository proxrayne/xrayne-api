using Api.Exceptions;
using Api.Requests;
using Api.Responses;
using AutoMapper;
using Contracts.Utilities;
using Contracts.Values;
using Infrastructure.Services;
using Infrastructure.States;
using Infrastructure.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Xray.Config.Share.v2Ray;

namespace Api.Controllers;

/// <summary>
/// Handles local xray-core status, installation, release lookup, and operations.
/// </summary>
[Route("api/core")]
[Authorize(Policy = AdminPermissionNames.SuperAdmin)]
[Authorize(Policy = AdminPermissionNames.ChangeXraySettings)]
public sealed class CoreController(
    IMapper mapper,
    IBackgroundTaskScheduler scheduler,
    ICoreStateMachine coreState,
    IEventStreamManager eventStreams,
    IMemoryCache cache) : ApiControllerBase
{
    private readonly GitHubReleaseClient xrayRepository = new(CoreDefaults.XrayRepositoryUrl);

    /// <summary>
    /// Returns the current local xray-core state.
    /// </summary>
    [HttpGet("status")]
    [EndpointSummary("Core status")]
    [EndpointDescription("Gets the current local xray-core status.")]
    [ProducesResponseType(typeof(CoreState), StatusCodes.Status200OK)]
    public CoreState GetStatus()
    {
        return coreState.GetCoreState();
    }

    /// <summary>
    /// Streams local xray-core state changes as server-sent events.
    /// </summary>
    [HttpGet("status/stream")]
    [EndpointSummary("Core status stream")]
    [EndpointDescription("Subscribe to Xray core status changes.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task StreamStatus(CancellationToken ct)
    {
        var subscription = eventStreams.Subscribe<CoreState>(CoreStateMachine.CoreStateStreamKey);

        SetupStreamHeaders();

        try
        {
            await Response.StartAsync(ct);
            await WriteServerSentEventAsync(coreState.GetCoreState(), ct);

            await foreach (var state in subscription.Reader.ReadAllAsync(ct))
            {
                await WriteServerSentEventAsync(state, ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        finally
        {
            eventStreams.Unsubscribe(subscription.Id);
        }
    }

    /// <summary>
    /// Returns available upstream xray-core releases.
    /// </summary>
    [HttpGet("releases")]
    [EndpointSummary("Xray releases")]
    [EndpointDescription("Get available Xray releases.")]
    [ProducesResponseType(typeof(List<GitHubReleaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<List<GitHubReleaseDto>> GetReleases([FromQuery] CoreReleasesQuery query, CancellationToken ct)
    {
        var releases = await cache.GetOrCreateAsync($"core_releases_{query.PerPage}_{query.Page}", entry =>
         {
             entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

             return xrayRepository.GetReleasesAsync(query.PerPage, query.Page, ct);
         });

        if (releases is null)
        {
            throw new BadRequestException("Failed to retrieve releases from cache.");
        }

        return releases.Select(mapper.Map<GitHubReleaseDto>).ToList();
    }

    [HttpGet("install/{jobId}/status")]
    [EndpointSummary("Install Xray status")]
    [EndpointDescription("Get the status of the Xray installation.")]
    [ProducesResponseType(typeof(InstallCoreState), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]

    public IActionResult GetInstallState(string jobId)
    {
        var state = coreState.GetInstallCoreState(jobId);
        if (state is null)
        {
            throw new NotFoundException($"Core by JobId = {jobId} not found.");
        }

        return Ok(state);
    }

    [HttpGet("install/{jobId}/stream")]
    [EndpointSummary("Install Xray state stream")]
    [EndpointDescription("Subscribe to Xray installation state changes.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task StreamInstallState(string jobId, CancellationToken ct)
    {
        var currentState = coreState.GetInstallCoreState(jobId);
        if (currentState is null)
        {
            throw new NotFoundException($"Core by JobId = {jobId} not found.");
        }

        var streamKey = CoreStateMachine.GetInstallCoreStreamKey(jobId);
        var subscription = eventStreams.Subscribe<InstallCoreState>(streamKey);

        SetupStreamHeaders();

        try
        {
            await Response.StartAsync(ct);
            await WriteServerSentEventAsync(currentState, ct);

            await foreach (var state in subscription.Reader.ReadAllAsync(ct))
            {
                await WriteServerSentEventAsync(state, ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        finally
        {
            eventStreams.Unsubscribe(subscription.Id);
        }
    }

    [HttpPost("install")]
    [EndpointSummary("Install Xray")]
    [EndpointDescription("Install the specified Xray version.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public Task<string> InstallCore([FromBody] InstallCoreRequest data, CancellationToken ct)
    {
        return scheduler.ScheduleInstallCore(data.Version ?? "latest", ct);
    }

    [HttpPost("start")]
    [EndpointSummary("Start Xray")]
    [EndpointDescription("Schedule Xray core start.")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartCore(CancellationToken ct)
    {
        await scheduler.ScheduleCoreOperation(CoreOperation.Start, ct);

        return Accepted();
    }

    [HttpPost("stop")]
    [EndpointSummary("Stop Xray")]
    [EndpointDescription("Schedule Xray core stop.")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StopCore(CancellationToken ct)
    {
        await scheduler.ScheduleCoreOperation(CoreOperation.Stop, ct);

        return Accepted();
    }

    [HttpPost("restart")]
    [EndpointSummary("Restart Xray")]
    [EndpointDescription("Schedule Xray core restart.")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestartCore(CancellationToken ct)
    {
        await scheduler.ScheduleCoreOperation(CoreOperation.Restart, ct);

        return Accepted();
    }

    [HttpPost("v2ray/decode")]
    [EndpointSummary("Decode v2ray link")]
    [EndpointDescription("Decode v2ray link to json outbound")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public IActionResult DecodeV2RayLink([FromBody] DecodeV2RayLinkRequest request)
    {
        var share = V2RayShareEntity.FromString(request.Link);
        var outbound = share.ToOutbound(remark: request.Remark, email: request.Email);
        if (outbound is null)
        {
            return BadRequest("Invalid operation result.");
        }

        return Ok(new { Outbound = XrayJsonSerializer.Serialize(outbound) });
    }
}
