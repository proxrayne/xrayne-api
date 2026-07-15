namespace Node.Models;

/// <summary>
/// Describes one remote node CPU core usage value.
/// </summary>
public sealed record CpuCoreUsage(
    int Index,
    double? UsagePercent);
