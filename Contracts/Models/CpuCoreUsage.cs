namespace Contracts.Models;

/// <summary>
/// Contains usage information for one logical CPU core.
/// </summary>
public sealed record CpuCoreUsage(
    int Index,
    double? UsagePercent);
