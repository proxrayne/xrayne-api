namespace RemoteNode.Models;

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
