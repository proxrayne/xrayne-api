namespace RemoteNode.Models;

/// <summary>
/// Describes one remote node CPU core usage value.
/// </summary>
public sealed record NodeCpuCoreUsage(
    int Index,
    double? UsagePercent);
