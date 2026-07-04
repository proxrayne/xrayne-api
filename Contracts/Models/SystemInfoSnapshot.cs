namespace Contracts.Models;

/// <summary>
/// Contains a point-in-time host system information snapshot.
/// </summary>
public sealed record SystemInfoSnapshot(
    CpuInfo Cpu,
    MemoryInfo Memory,
    SwapInfo Swap,
    StorageInfo Storage,
    TimeSpan Uptime,
    int CurrentProcessThreadCount,
    long? SystemThreadCount,
    NetworkInfo Network);
