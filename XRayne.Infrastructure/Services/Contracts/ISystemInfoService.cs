using XRayne.Infrastructure.Models;

namespace XRayne.Infrastructure.Services;

public interface ISystemInfoService
{
    Task<SystemInfoSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);

    Task<CpuInfo> GetCpuInfoAsync(CancellationToken cancellationToken = default);

    Task<MemoryInfo> GetMemoryInfoAsync(CancellationToken cancellationToken = default);

    Task<SwapInfo> GetSwapInfoAsync(CancellationToken cancellationToken = default);

    StorageInfo GetStorageInfo();

    TimeSpan GetUptime();

    int GetCurrentProcessThreadCount();

    Task<long?> GetSystemThreadCountAsync(CancellationToken cancellationToken = default);

    NetworkInfo GetNetworkInfo();
}
