namespace Contracts.Models;

/// <summary>
/// Contains a directory path and its calculated size.
/// </summary>
public sealed record DirectorySizeInfo(
    string Path,
    long SizeBytes);
