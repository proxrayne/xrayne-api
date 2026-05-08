using System.Diagnostics;
using System.Globalization;
using XRayne.Contracts.Values;
using XRayne.Infrastructure.Models;
using XRayne.Infrastructure.Utilities;

namespace XRayne.Infrastructure.Services;

public abstract class SystemInfoService : ISystemInfoService
{
    public async Task<SystemInfoSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var cpuTask = GetCpuInfoAsync(cancellationToken);
        var memoryTask = GetMemoryInfoAsync(cancellationToken);
        var swapTask = GetSwapInfoAsync(cancellationToken);
        var systemThreadCountTask = GetSystemThreadCountAsync(cancellationToken);

        await Task.WhenAll(cpuTask, memoryTask, swapTask, systemThreadCountTask);

        return new SystemInfoSnapshot(
            cpuTask.Result,
            memoryTask.Result,
            swapTask.Result,
            GetDiskInfo(),
            GetUptime(),
            GetCurrentProcessThreadCount(),
            systemThreadCountTask.Result,
            GetNetworkInfo());
    }

    public abstract Task<CpuInfo> GetCpuInfoAsync(CancellationToken cancellationToken = default);

    public abstract Task<MemoryInfo> GetMemoryInfoAsync(CancellationToken cancellationToken = default);

    public abstract Task<SwapInfo> GetSwapInfoAsync(CancellationToken cancellationToken = default);

    public abstract Task<long?> GetSystemThreadCountAsync(CancellationToken cancellationToken = default);

    public TimeSpan GetUptime() => TimeSpan.FromMilliseconds(Environment.TickCount64);

    public int GetCurrentProcessThreadCount() => Process.GetCurrentProcess().Threads.Count;

    protected static CpuInfo CreateCpuInfo(IReadOnlyCollection<double?> usageByCore)
    {
        var cores = usageByCore
            .Select((usage, index) => new CpuCoreUsage(index, usage))
            .ToArray();

        return new CpuInfo(Environment.ProcessorCount, cores);
    }

    protected static CpuInfo CreateCpuInfoWithoutUsage()
    {
        var cores = Enumerable
            .Range(0, Environment.ProcessorCount)
            .Select(index => new CpuCoreUsage(index, null))
            .ToArray();

        return new CpuInfo(Environment.ProcessorCount, cores);
    }

    protected static long KilobytesToBytes(long value) => value * 1024;

    protected static long MegabytesToBytes(long value) => value * 1024 * 1024;

    protected static double ClampPercent(double value) => Math.Clamp(value, 0, 100);

    protected static bool TryParseLong(string? value, out long result)
    {
        return long.TryParse(
            value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out result);
    }

    protected static async Task<string> RunProcessAsync(
        string fileName,
        string arguments,
        CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return await outputTask;
    }

    public IReadOnlyCollection<DiskInfo> GetDiskInfo()
    {
        return DriveInfo.GetDrives()
            .Where(drive => drive.IsReady)
            .Select(drive => new DiskInfo(
                drive.Name,
                drive.RootDirectory.FullName,
                drive.DriveFormat,
                drive.TotalSize,
                drive.TotalSize - drive.AvailableFreeSpace,
                drive.AvailableFreeSpace))
            .OrderByDescending(drive => IsProjectDrive(drive.RootDirectory))
            .ThenBy(drive => drive.RootDirectory, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool IsProjectDrive(string rootDirectory)
    {
        var projectRoot = PathProvider.Paths.Root;

        return projectRoot.StartsWith(rootDirectory, StringComparison.OrdinalIgnoreCase);
    }

    public NetworkInfo GetNetworkInfo()
    {
        var addresses = NetworkAddress.GetServerIpAddresses();

        return new NetworkInfo(addresses.IPv4Addresses, addresses.IPv6Addresses);
    }
}
