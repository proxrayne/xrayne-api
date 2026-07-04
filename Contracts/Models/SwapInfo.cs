namespace Contracts.Models;

/// <summary>
/// Contains swap or pagefile information.
/// </summary>
public sealed record SwapInfo(
    long TotalBytes,
    long UsedBytes,
    long AvailableBytes);
