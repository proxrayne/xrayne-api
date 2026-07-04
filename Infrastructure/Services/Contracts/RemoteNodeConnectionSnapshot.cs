using RemoteNode.Models;

namespace Infrastructure.Services;

/// <summary>
/// Describes a live remote node connection snapshot held in panel memory.
/// </summary>
public sealed record RemoteNodeConnectionSnapshot(
    long NodeId,
    RemoteNodeConnectionState State,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ConnectedAt,
    DateTimeOffset? LastHeartbeatAt,
    int ReconnectAttemptCount,
    string? Message,
    NodePingResponse? Telemetry);
