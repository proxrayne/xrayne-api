namespace XRayne.Infrastructure.Models;

public sealed record SystemInfoSnapshot(
    CpuInfo Cpu,
    MemoryInfo Memory,
    SwapInfo Swap,
    IReadOnlyCollection<DiskInfo> Disks,
    TimeSpan Uptime,
    int CurrentProcessThreadCount,
    long? SystemThreadCount,
    NetworkInfo Network);

public sealed record CpuInfo(
    int LogicalCoreCount,
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

public sealed record DiskInfo(
    string Name,
    string RootDirectory,
    string DriveFormat,
    long TotalBytes,
    long UsedBytes,
    long AvailableBytes);

public sealed record NetworkInfo(
    IReadOnlyCollection<string> IPv4Addresses,
    IReadOnlyCollection<string> IPv6Addresses);
