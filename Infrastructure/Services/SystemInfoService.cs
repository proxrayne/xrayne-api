using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Contracts.Models;
using Hardware.Info;
using MemoryInfo = Contracts.Models.MemoryInfo;

namespace Infrastructure.Services;

/// <summary>
/// Reads host system information through Hardware.Info and small BCL fallbacks.
/// </summary>
public sealed class SystemInfoService : ISystemInfoService
{
    private readonly SystemInfoOptions _options;

    /// <summary>
    /// Initializes a system information service.
    /// </summary>
    public SystemInfoService(SystemInfoOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApplicationDirectory))
        {
            throw new ArgumentException("Application directory cannot be empty.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.DownloadsDirectory))
        {
            throw new ArgumentException("Downloads directory cannot be empty.", nameof(options));
        }

        _options = options;
    }

    /// <inheritdoc />
    public async Task<SystemInfoSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var cpuTask = GetCpuInfoAsync(cancellationToken);
        var memoryTask = GetMemoryInfoAsync(cancellationToken);
        var swapTask = GetSwapInfoAsync(cancellationToken);
        var threadTask = GetSystemThreadCountAsync(cancellationToken);

        await Task.WhenAll(cpuTask, memoryTask, swapTask, threadTask);

        return new SystemInfoSnapshot(
            cpuTask.Result,
            memoryTask.Result,
            swapTask.Result,
            GetStorageInfo(),
            GetUptime(),
            GetCurrentProcessThreadCount(),
            threadTask.Result,
            GetNetworkInfo());
    }

    /// <inheritdoc />
    public Task<CpuInfo> GetCpuInfoAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var hardwareInfo = new HardwareInfo();
        try
        {
            hardwareInfo.RefreshCPUList(
                includePercentProcessorTime: true,
                millisecondsDelayBetweenTwoMeasurements: 250,
                includePerformanceCounter: false);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Task.FromResult(CreateCpuInfoWithoutUsage());
        }

        cancellationToken.ThrowIfCancellationRequested();

        var logicalCoreCount = hardwareInfo.CpuList
            .Select(cpu => SafeToInt(cpu.NumberOfLogicalProcessors))
            .Where(count => count > 0)
            .DefaultIfEmpty(Environment.ProcessorCount)
            .Sum();

        if (logicalCoreCount <= 0)
        {
            logicalCoreCount = Environment.ProcessorCount;
        }

        var coreUsages = hardwareInfo.CpuList
            .SelectMany(cpu => cpu.CpuCoreList)
            .Select(core => (double?)ClampPercent(core.PercentProcessorTime))
            .ToArray();

        if (coreUsages.Length == 0)
        {
            var cpuUsage = hardwareInfo.CpuList
                .Select(cpu => (double?)ClampPercent(cpu.PercentProcessorTime))
                .FirstOrDefault();

            var cores = Enumerable
                .Range(0, logicalCoreCount)
                .Select(index => new CpuCoreUsage(index, cpuUsage))
                .ToArray();

            return Task.FromResult(new CpuInfo(logicalCoreCount, cpuUsage, cores));
        }

        var values = coreUsages
            .Where(usage => usage.HasValue)
            .Select(usage => usage!.Value)
            .ToArray();
        var average = values.Length == 0
            ? null
            : (double?)ClampPercent(values.Average());
        var result = new CpuInfo(
            logicalCoreCount,
            average,
            coreUsages.Select((usage, index) => new CpuCoreUsage(index, usage)).ToArray());

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<MemoryInfo> GetMemoryInfoAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var hardwareInfo = new HardwareInfo();
        try
        {
            hardwareInfo.RefreshMemoryStatus();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Task.FromResult(new MemoryInfo(0, 0, 0));
        }

        var total = SafeToLong(hardwareInfo.MemoryStatus.TotalPhysical);
        var available = SafeToLong(hardwareInfo.MemoryStatus.AvailablePhysical);

        return Task.FromResult(new MemoryInfo(total, Math.Max(0, total - available), available));
    }

    /// <inheritdoc />
    public Task<SwapInfo> GetSwapInfoAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var hardwareInfo = new HardwareInfo();
        try
        {
            hardwareInfo.RefreshMemoryStatus();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Task.FromResult(new SwapInfo(0, 0, 0));
        }

        var total = SafeToLong(hardwareInfo.MemoryStatus.TotalPageFile);
        var available = SafeToLong(hardwareInfo.MemoryStatus.AvailablePageFile);

        return Task.FromResult(new SwapInfo(total, Math.Max(0, total - available), available));
    }

    /// <inheritdoc />
    public StorageInfo GetStorageInfo()
    {
        var applicationDirectorySize = GetDirectorySize(_options.ApplicationDirectory);
        var downloadsDirectorySize = GetDirectorySize(_options.DownloadsDirectory);
        var applicationDirectoryUsedDiskPercent = GetDirectoryUsedDiskPercent(
            _options.ApplicationDirectory,
            applicationDirectorySize);

        return new StorageInfo(
            new DirectorySizeInfo(_options.ApplicationDirectory, applicationDirectorySize),
            new DirectorySizeInfo(_options.DownloadsDirectory, downloadsDirectorySize),
            applicationDirectoryUsedDiskPercent);
    }

    /// <inheritdoc />
    public TimeSpan GetUptime() => TimeSpan.FromMilliseconds(Environment.TickCount64);

    /// <inheritdoc />
    public int GetCurrentProcessThreadCount() => Process.GetCurrentProcess().Threads.Count;

    /// <inheritdoc />
    public Task<long?> GetSystemThreadCountAsync(CancellationToken cancellationToken = default)
    {
        if (OperatingSystem.IsLinux())
        {
            return Task.FromResult(GetLinuxSystemThreadCount(cancellationToken));
        }

        return Task.FromResult<long?>(null);
    }

    /// <inheritdoc />
    public NetworkInfo GetNetworkInfo()
    {
        var hardwareInfo = new HardwareInfo();
        try
        {
            hardwareInfo.RefreshNetworkAdapterList(
                includeBytesPerSec: false,
                includeNetworkAdapterConfiguration: true,
                millisecondsDelayBetweenTwoMeasurements: 0);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
        }

        var hardwareAddresses = hardwareInfo.NetworkAdapterList
            .SelectMany(adapter => adapter.IPAddressList)
            .Where(address => address is not null);
        var fallbackAddresses = GetUsableServerAddresses();
        var addresses = hardwareAddresses
            .Concat(fallbackAddresses)
            .Distinct()
            .ToArray();

        var ipv4 = addresses
            .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
            .Where(address => !IPAddress.IsLoopback(address))
            .Select(address => address.ToString())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order()
            .ToArray();

        var ipv6 = addresses
            .Where(address => address.AddressFamily == AddressFamily.InterNetworkV6)
            .Where(IsUsableIPv6Address)
            .Select(address => address.ToString())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order()
            .ToArray();

        return new NetworkInfo(ipv4, ipv6);
    }

    private static CpuInfo CreateCpuInfoWithoutUsage()
    {
        var cores = Enumerable
            .Range(0, Environment.ProcessorCount)
            .Select(index => new CpuCoreUsage(index, null))
            .ToArray();

        return new CpuInfo(Environment.ProcessorCount, null, cores);
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

    private static double GetDirectoryUsedDiskPercent(string directoryPath, long directorySize)
    {
        try
        {
            var hardwareInfo = new HardwareInfo();
            hardwareInfo.RefreshDriveList();
            var root = Path.GetPathRoot(directoryPath);
            var volume = hardwareInfo.DriveList
                .SelectMany(drive => drive.PartitionList)
                .SelectMany(partition => partition.VolumeList)
                .FirstOrDefault(item => !string.IsNullOrWhiteSpace(root)
                    && item.Name.StartsWith(root, StringComparison.OrdinalIgnoreCase));

            if (volume is not null && volume.Size > 0)
            {
                return ClampPercent(directorySize / (double)volume.Size * 100);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
        }

        try
        {
            var root = Path.GetPathRoot(directoryPath) ?? directoryPath;
            var drive = new DriveInfo(root);

            return drive.TotalSize <= 0
                ? 0
                : ClampPercent(directorySize / (double)drive.TotalSize * 100);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return 0;
        }
    }

    private static long? GetLinuxSystemThreadCount(CancellationToken cancellationToken)
    {
        const string procPath = "/proc";
        if (!Directory.Exists(procPath))
        {
            return null;
        }

        long count = 0;
        foreach (var processDirectory in Directory.EnumerateDirectories(procPath))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var directoryName = Path.GetFileName(processDirectory);
            if (!directoryName.All(char.IsDigit))
            {
                continue;
            }

            var statusFile = Path.Combine(processDirectory, "status");
            if (TryReadThreadCount(statusFile, out var threadCount))
            {
                count += threadCount;
            }
        }

        return count;
    }

    private static bool TryReadThreadCount(string statusFile, out long count)
    {
        count = 0;

        try
        {
            foreach (var line in File.ReadLines(statusFile))
            {
                if (!line.StartsWith("Threads:", StringComparison.Ordinal))
                {
                    continue;
                }

                var value = line["Threads:".Length..].Trim();
                return long.TryParse(value, out count);
            }
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }

        return false;
    }

    private static IEnumerable<IPAddress> GetUsableServerAddresses()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(item => item.OperationalStatus == OperationalStatus.Up)
            .Where(item => item.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .SelectMany(item => item.GetIPProperties().UnicastAddresses)
            .Select(item => item.Address)
            .Where(address => !IPAddress.IsLoopback(address));
    }

    private static bool IsUsableIPv6Address(IPAddress address)
    {
        return address.AddressFamily == AddressFamily.InterNetworkV6
            && !IPAddress.IsLoopback(address)
            && !address.IsIPv6LinkLocal
            && !address.IsIPv6Multicast;
    }

    private static int SafeToInt(uint value) => value > int.MaxValue ? int.MaxValue : (int)value;

    private static long SafeToLong(ulong value) => value > long.MaxValue ? long.MaxValue : (long)value;

    private static double ClampPercent(double value) => Math.Clamp(value, 0, 100);
}


/// <summary>
/// Defines host directories used by system information storage metrics.
/// </summary>
public sealed record SystemInfoOptions(
    string ApplicationDirectory,
    string DownloadsDirectory);
