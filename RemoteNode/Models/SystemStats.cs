namespace RemoteNode.Models;

/// <summary>
/// Describes basic remote node system statistics.
/// </summary>
public sealed record SystemStats(
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
    CpuStats Cpu,
    MemoryStats Memory,
    MemoryStats Swap,
    IReadOnlyCollection<VolumeStats> Volumes,
    NetworkStats Network);
