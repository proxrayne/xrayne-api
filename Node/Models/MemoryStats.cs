namespace Node.Models;

/// <summary>
/// Describes memory or swap usage on a remote node.
/// </summary>
public sealed record MemoryStats(
    long TotalBytes,
    long UsedBytes,
    long AvailableBytes);
