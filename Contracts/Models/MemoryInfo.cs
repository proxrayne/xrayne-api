namespace Contracts.Models;

/// <summary>
/// Contains physical memory information.
/// </summary>
public sealed record MemoryInfo(
    long TotalBytes,
    long UsedBytes,
    long AvailableBytes);
