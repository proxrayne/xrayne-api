namespace Contracts.Models;

/// <summary>
/// Contains CPU topology and usage information.
/// </summary>
public sealed record CpuInfo(
    int LogicalCoreCount,
    double? AverageUsagePercent,
    IReadOnlyCollection<CpuCoreUsage> Cores);
