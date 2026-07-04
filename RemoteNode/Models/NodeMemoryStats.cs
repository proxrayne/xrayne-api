namespace RemoteNode.Models;

/// <summary>
/// Describes memory or swap usage on a remote node.
/// </summary>
public sealed record NodeMemoryStats(
    long TotalBytes,
    long UsedBytes,
    long AvailableBytes);
