namespace Node.Responses;

/// <summary>
/// Describes the current remote node telemetry returned to the panel.
/// </summary>
public sealed record NodePingResponse(
    string Service,
    string NodeVersion,
    string Environment,
    DateTimeOffset StartedAt,
    DateTimeOffset Timestamp,
    TimeSpan Uptime,
    NodeCoreStatus Core,
    NodeSystemStats System);

/// <summary>
/// Describes the current xray-core state on the remote node.
/// </summary>
public sealed record NodeCoreStatus(
    bool IsInstalled,
    bool IsRunning,
    string? Version,
    string Status);

/// <summary>
/// Describes basic remote node system statistics.
/// </summary>
public sealed record NodeSystemStats(
    string MachineName,
    string OSDescription,
    int ProcessorCount,
    long WorkingSetBytes,
    long GcTotalMemoryBytes);

/// <summary>
/// Describes a node connection stream event.
/// </summary>
public sealed record NodeConnectionEvent(
    string Type,
    DateTimeOffset Timestamp,
    NodePingResponse? Ping);
