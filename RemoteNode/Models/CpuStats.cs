namespace RemoteNode.Models;

/// <summary>
/// Describes remote node CPU usage.
/// </summary>
public sealed record CpuStats(
    int LogicalCoreCount,
    double? AverageUsagePercent,
    IReadOnlyCollection<CpuCoreUsage> Cores);
