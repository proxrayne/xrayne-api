namespace XRayne.Infrastructure.Models;

public sealed record SystemInfoSnapshot(
    CpuInfo Cpu,
    MemoryInfo Memory,
    SwapInfo Swap,
    StorageInfo Storage,
    TimeSpan Uptime,
    int CurrentProcessThreadCount,
    long? SystemThreadCount,
    NetworkInfo Network);

public sealed record CpuInfo(
    int LogicalCoreCount,
    double? AverageUsagePercent,
    IReadOnlyCollection<CpuCoreUsage> Cores);

public sealed record CpuCoreUsage(
    int Index,
    double? UsagePercent);

public sealed record MemoryInfo(
    long TotalBytes,
    long UsedBytes,
    long AvailableBytes);

public sealed record SwapInfo(
    long TotalBytes,
    long UsedBytes,
    long AvailableBytes);

public sealed record StorageInfo(
    DirectorySizeInfo ApplicationDirectory,
    DirectorySizeInfo DownloadsDirectory,
    double ApplicationDirectoryUsedDiskPercent);

public sealed record DirectorySizeInfo(
    string Path,
    long SizeBytes);

public sealed record NetworkInfo(
    IReadOnlyCollection<string> IPv4Addresses,
    IReadOnlyCollection<string> IPv6Addresses);
