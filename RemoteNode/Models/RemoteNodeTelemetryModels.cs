namespace RemoteNode.Models;

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
    long GcTotalMemoryBytes,
    int CurrentProcessThreadCount,
    long? SystemThreadCount,
    DateTimeOffset StartedAt,
    DateTimeOffset Timestamp,
    TimeSpan Uptime,
    NodeCpuStats Cpu,
    NodeMemoryStats Memory,
    NodeMemoryStats Swap,
    IReadOnlyCollection<NodeVolumeStats> Volumes,
    NodeNetworkStats Network);

/// <summary>
/// Describes remote node CPU usage.
/// </summary>
public sealed record NodeCpuStats(
    int LogicalCoreCount,
    double? AverageUsagePercent,
    IReadOnlyCollection<NodeCpuCoreUsage> Cores);

/// <summary>
/// Describes one remote node CPU core usage value.
/// </summary>
public sealed record NodeCpuCoreUsage(
    int Index,
    double? UsagePercent);

/// <summary>
/// Describes memory or swap usage on a remote node.
/// </summary>
public sealed record NodeMemoryStats(
    long TotalBytes,
    long UsedBytes,
    long AvailableBytes);

/// <summary>
/// Describes a mounted volume on a remote node.
/// </summary>
public sealed record NodeVolumeStats(
    string Name,
    string FileSystem,
    long TotalBytes,
    long FreeBytes,
    long UsedBytes,
    double UsedPercent);

/// <summary>
/// Describes remote node network addresses.
/// </summary>
public sealed record NodeNetworkStats(
    IReadOnlyCollection<string> IPv4Addresses,
    IReadOnlyCollection<string> IPv6Addresses);

/// <summary>
/// Describes a node connection stream event.
/// </summary>
public sealed record NodeConnectionEvent(
    string Type,
    DateTimeOffset Timestamp,
    NodePingResponse? Ping);
