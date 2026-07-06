using Contracts.Enums;

namespace Api.Responses;

/// <summary>
/// Response model for cached remote node connection telemetry.
/// </summary>
public sealed record NodeConnectionSnapshotResponse(
    long NodeId,
    NodeConnectionStatus State,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ConnectedAt,
    DateTimeOffset? LastHeartbeatAt,
    int ReconnectAttemptCount,
    string? Message,
    string? ApiVersion,
    DateTimeOffset? Uptime);
