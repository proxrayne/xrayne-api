using Infrastructure.Services;
using RemoteNode.Models;

namespace Api.Responses;

/// <summary>
/// Response model for cached remote node connection telemetry.
/// </summary>
public sealed record NodeConnectionSnapshotResponse(
    long NodeId,
    RemoteNodeConnectionState State,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ConnectedAt,
    DateTimeOffset? LastHeartbeatAt,
    int ReconnectAttemptCount,
    string? Message,
    NodePingResponse? Telemetry);
