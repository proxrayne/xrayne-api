using System.Diagnostics;
using System.Globalization;
using XRayne.Contracts.Values;
using XRayne.Infrastructure.Models;
using XRayne.Infrastructure.Utilities;

namespace XRayne.Infrastructure.Services;

public abstract class SystemInfoService(IProjectPathResolver paths) : ISystemInfoService
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
            GetStorageInfo(),
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
        var values = cores
            .Where(core => core.UsagePercent.HasValue)
            .Select(core => core.UsagePercent!.Value)
            .ToArray();
        var average = values.Length == 0
            ? null
            : (double?)ClampPercent(values.Average());

        return new CpuInfo(Environment.ProcessorCount, average, cores);
    }

    protected static CpuInfo CreateCpuInfoWithoutUsage()
    {
        var cores = Enumerable
            .Range(0, Environment.ProcessorCount)
            .Select(index => new CpuCoreUsage(index, null))
            .ToArray();

        return new CpuInfo(Environment.ProcessorCount, null, cores);
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
        CancellationToken cancellationToken,
        bool createNoWindow = true)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = createNoWindow
        };

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return await outputTask;
    }

    public StorageInfo GetStorageInfo()
    {
        var applicationDirectorySize = GetDirectorySize(paths.Root);
        var downloadsDirectorySize = GetDirectorySize(paths.DownloadsDirectory);
        var applicationDrive = new DriveInfo(Path.GetPathRoot(paths.Root) ?? paths.Root);
        var applicationDirectoryUsedDiskPercent = applicationDrive.TotalSize <= 0
            ? 0
            : ClampPercent(applicationDirectorySize / (double)applicationDrive.TotalSize * 100);

        return new StorageInfo(
            new DirectorySizeInfo(paths.Root, applicationDirectorySize),
            new DirectorySizeInfo(paths.DownloadsDirectory, downloadsDirectorySize),
            applicationDirectoryUsedDiskPercent);
    }

    public NetworkInfo GetNetworkInfo()
    {
        var addresses = NetworkAddress.GetServerIpAddresses();

        return new NetworkInfo(addresses.IPv4Addresses, addresses.IPv6Addresses);
    }

    private static long GetDirectorySize(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return 0;
        }

        long size = 0;
        var pending = new Stack<string>();
        pending.Push(directoryPath);

        while (pending.Count > 0)
        {
            var current = pending.Pop();

            try
            {
                foreach (var file in Directory.EnumerateFiles(current))
                {
                    try
                    {
                        size += new FileInfo(file).Length;
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }

                foreach (var directory in Directory.EnumerateDirectories(current))
                {
                    pending.Push(directory);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        return size;
    }
}
