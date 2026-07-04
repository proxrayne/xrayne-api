namespace RemoteNode.Models;

/// <summary>
/// Describes a mounted volume on a remote node.
/// </summary>
public sealed record NodeVolumeStats(
    string Name,
    string FileSystem,
    long TotalBytes,
    long FreeBytes,
    long UsedBytes,
    double UsedPercent);
