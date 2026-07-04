namespace RemoteNode.Models;

/// <summary>
/// Describes remote node CPU usage.
/// </summary>
public sealed record NodeCpuStats(
    int LogicalCoreCount,
    double? AverageUsagePercent,
    IReadOnlyCollection<NodeCpuCoreUsage> Cores);
