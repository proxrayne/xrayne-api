namespace Contracts.Models;

/// <summary>
/// Contains storage usage information for configured directories.
/// </summary>
public sealed record StorageInfo(
    DirectorySizeInfo ApplicationDirectory,
    DirectorySizeInfo DownloadsDirectory,
    double ApplicationDirectoryUsedDiskPercent);
