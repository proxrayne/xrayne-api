using Api.Exceptions;
using Api.Responses;
using Contracts.Values;
using Data.Contracts;
using Infrastructure.Services;
using Infrastructure.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Node.Models;

namespace Api.Controllers;

/// <summary>
/// Exposes live remote xray-core logs.
/// </summary>
[Authorize(Policy = AdminPermissionNames.ViewLogs)]
[Route("api/nodes/{id:long}/logs")]
public sealed class NodeLogsController(
    INodeRepository nodeRepository,
    INodeLogStore nodeLogs,
    IEventStreamManager eventStreams) : ApiControllerBase
{
    /// <summary>
    /// Gets recent remote xray-core log entries.
    /// </summary>
    [HttpGet]
    [EndpointSummary("Remote xray-core logs")]
    [EndpointDescription("Get recent live xray-core log entries received from a remote node.")]
    [ProducesResponseType(typeof(RemoteLogSnapshotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<RemoteLogSnapshotResponse> GetLogs(
        long id,
        [FromQuery] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureNodeExistsAsync(id, cancellationToken);
        var normalizedLimit = nodeLogs.NormalizeLimit(limit);

        return new RemoteLogSnapshotResponse(normalizedLimit, nodeLogs.Get(id, normalizedLimit));
    }

    /// <summary>
    /// Streams remote xray-core log entries.
    /// </summary>
    [HttpGet("stream")]
    [EndpointSummary("Remote xray-core logs stream")]
    [EndpointDescription("Subscribe to live xray-core log entries received from a remote node.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task StreamLogs(
        long id,
        [FromQuery] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureNodeExistsAsync(id, cancellationToken);
        var normalizedLimit = nodeLogs.NormalizeLimit(limit);
        var subscription = eventStreams.Subscribe<RemoteLogStreamEvent>(NodeLogStreamKeys.ForNode(id));

        SetupStreamHeaders();

        try
        {
            await Response.StartAsync(cancellationToken);
            await WriteServerSentEventAsync(
                new RemoteLogStreamEvent(
                    "snapshot",
                    nodeLogs.Get(id, normalizedLimit),
                    null,
                    Source: "panel"),
                cancellationToken);

            await foreach (var logEvent in subscription.Reader.ReadAllAsync(cancellationToken))
            {
                await WriteServerSentEventAsync(logEvent, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        finally
        {
            eventStreams.Unsubscribe(subscription.Id);
        }
    }

    private async Task EnsureNodeExistsAsync(long id, CancellationToken ct)
    {
        if (!await nodeRepository.ExistByIdAsync(id, ct))
        {
            throw new NotFoundException($"Node '{id}' was not found.");
        }
    }
}
