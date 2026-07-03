using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Node.Responses;
using SystemInfo;

namespace Node.Controllers;

/// <summary>
/// Exposes remote node health and connection stream endpoints for the panel.
/// </summary>
[ApiController]
[Route("api")]
public sealed class ConnectionController(SystemInfoService systemInfo, IConfiguration configuration) : ApiControllerBase
{
    private static readonly JsonSerializerOptions SseJsonOptions = new(JsonSerializerDefaults.Web);

    // /// <summary>
    // /// Gets current remote node telemetry.
    // /// </summary>
    // [HttpGet("ping")]
    // [EndpointSummary("Ping node")]
    // [EndpointDescription("Get the current remote node telemetry used by the panel for liveness checks.")]
    // [ProducesResponseType(typeof(NodePingResponse), StatusCodes.Status200OK)]
    // public NodePingResponse Ping()
    // {
    //     var info = systemInfo.GetSnapshotAsync();

    //     return systemInfo.GetPing();
    // }

    // /// <summary>
    // /// Opens a remote node event stream.
    // /// </summary>
    // [HttpGet("connect")]
    // [EndpointSummary("Connect node stream")]
    // [EndpointDescription("Subscribe to remote node heartbeat and runtime events.")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task Connect(CancellationToken cancellationToken)
    // {
    //     SetupStreamHeaders();
    //     await Response.StartAsync(cancellationToken);

    //     await WriteServerSentEventAsync(
    //         new NodeConnectionEvent("connected", DateTimeOffset.UtcNow, telemetry.GetPing()),
    //         cancellationToken);

    //     var heartbeatSeconds = configuration.GetValue("Node:StreamHeartbeatSeconds", 15);
    //     var heartbeatDelay = TimeSpan.FromSeconds(Math.Max(1, heartbeatSeconds));

    //     using var timer = new PeriodicTimer(heartbeatDelay);
    //     while (await timer.WaitForNextTickAsync(cancellationToken))
    //     {
    //         await WriteServerSentEventAsync(
    //             new NodeConnectionEvent("heartbeat", DateTimeOffset.UtcNow, telemetry.GetPing()),
    //             cancellationToken);
    //     }
    // }
}
