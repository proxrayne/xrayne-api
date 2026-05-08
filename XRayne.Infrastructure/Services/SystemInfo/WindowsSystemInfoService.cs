using System.Globalization;
using XRayne.Infrastructure.Models;

namespace XRayne.Infrastructure.Services;

public sealed class WindowsSystemInfoService : SystemInfoService
{
    public override async Task<CpuInfo> GetCpuInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var output = await RunPowerShellAsync(
                "(Get-Counter '\\Processor(*)\\% Processor Time').CounterSamples | Where-Object {$_.InstanceName -ne '_total'} | Sort-Object {[int]$_.InstanceName} | ForEach-Object {$_.CookedValue.ToString([System.Globalization.CultureInfo]::InvariantCulture)}",
                cancellationToken);

            var usages = output
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => double.TryParse(value, CultureInfo.InvariantCulture, out var parsed)
                    ? ClampPercent(parsed)
                    : (double?)null)
                .ToArray();

            return usages.Length == 0
                ? CreateCpuInfoWithoutUsage()
                : CreateCpuInfo(usages);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return CreateCpuInfoWithoutUsage();
        }
    }

    public override async Task<MemoryInfo> GetMemoryInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var output = await RunPowerShellAsync(
                "$os = Get-CimInstance Win32_OperatingSystem; [string]::Join('|', @($os.TotalVisibleMemorySize, $os.FreePhysicalMemory))",
                cancellationToken);
            var values = output.Trim().Split('|');

            if (values.Length >= 2
                && TryParseLong(values[0], out var totalKb)
                && TryParseLong(values[1], out var freeKb))
            {
                var total = KilobytesToBytes(totalKb);
                var free = KilobytesToBytes(freeKb);

                return new MemoryInfo(total, Math.Max(0, total - free), free);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
        }

        return new MemoryInfo(0, 0, 0);
    }

    public override async Task<SwapInfo> GetSwapInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var output = await RunPowerShellAsync(
                "$items = Get-CimInstance Win32_PageFileUsage; [string]::Join('|', @(($items | Measure-Object AllocatedBaseSize -Sum).Sum, ($items | Measure-Object CurrentUsage -Sum).Sum))",
                cancellationToken);
            var values = output.Trim().Split('|');

            if (values.Length >= 2
                && TryParseLong(values[0], out var totalMb)
                && TryParseLong(values[1], out var usedMb))
            {
                var total = MegabytesToBytes(totalMb);
                var used = MegabytesToBytes(usedMb);

                return new SwapInfo(total, used, Math.Max(0, total - used));
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
        }

        return new SwapInfo(0, 0, 0);
    }

    public override async Task<long?> GetSystemThreadCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var output = await RunPowerShellAsync(
                "(Get-CimInstance Win32_PerfRawData_PerfOS_System).Threads",
                cancellationToken);

            return TryParseLong(output.Trim(), out var count) ? count : null;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return null;
        }
    }

    private static Task<string> RunPowerShellAsync(string command, CancellationToken cancellationToken)
    {
        var escapedCommand = command.Replace("\"", "\\\"", StringComparison.Ordinal);

        return RunProcessAsync(
            "powershell",
            $"-NoProfile -ExecutionPolicy Bypass -Command \"{escapedCommand}\"",
            cancellationToken);
    }
}
