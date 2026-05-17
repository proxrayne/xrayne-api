using XRayne.Contracts.Values;
using XRayne.Infrastructure.Services;
using XRayne.Infrastructure.Services.PanelSettings;

namespace XRayne.Test.Infrastructure;

public sealed class SystemInfoServiceTests
{
    [Fact]
    public async Task GetSnapshotAsync_ReturnsSystemInfo()
    {
        var service = CreateService();

        var snapshot = await service.GetSnapshotAsync();

        Assert.True(snapshot.Cpu.LogicalCoreCount > 0);
        Assert.NotEmpty(snapshot.Cpu.Cores);
        if (snapshot.Cpu.AverageUsagePercent.HasValue)
        {
            Assert.InRange(snapshot.Cpu.AverageUsagePercent.Value, 0, 100);
        }

        Assert.All(snapshot.Cpu.Cores, core =>
        {
            if (core.UsagePercent.HasValue)
            {
                Assert.InRange(core.UsagePercent.Value, 0, 100);
            }
        });

        Assert.True(snapshot.Memory.TotalBytes >= 0);
        Assert.True(snapshot.Memory.UsedBytes >= 0);
        Assert.True(snapshot.Memory.AvailableBytes >= 0);
        Assert.True(snapshot.Swap.TotalBytes >= 0);
        Assert.True(snapshot.Swap.UsedBytes >= 0);
        Assert.True(snapshot.Swap.AvailableBytes >= 0);
        Assert.True(snapshot.Storage.ApplicationDirectory.SizeBytes >= 0);
        Assert.True(snapshot.Storage.DownloadsDirectory.SizeBytes >= 0);
        Assert.InRange(snapshot.Storage.ApplicationDirectoryUsedDiskPercent, 0, 100);
        Assert.True(snapshot.Uptime > TimeSpan.Zero);
        Assert.True(snapshot.CurrentProcessThreadCount > 0);
        Assert.NotNull(snapshot.Network.IPv4Addresses);
        Assert.NotNull(snapshot.Network.IPv6Addresses);
    }

    [Fact]
    public async Task ParameterMethods_ReturnSystemInfoParts()
    {
        var service = CreateService();

        var cpu = await service.GetCpuInfoAsync();
        var memory = await service.GetMemoryInfoAsync();
        var swap = await service.GetSwapInfoAsync();
        var storage = service.GetStorageInfo();
        var uptime = service.GetUptime();
        var currentProcessThreadCount = service.GetCurrentProcessThreadCount();
        var systemThreadCount = await service.GetSystemThreadCountAsync();
        var network = service.GetNetworkInfo();

        Assert.True(cpu.LogicalCoreCount > 0);
        Assert.NotEmpty(cpu.Cores);
        if (cpu.AverageUsagePercent.HasValue)
        {
            Assert.InRange(cpu.AverageUsagePercent.Value, 0, 100);
        }

        if (OperatingSystem.IsWindows())
        {
            Assert.Contains(cpu.Cores, core => core.UsagePercent.HasValue);
            Assert.NotNull(cpu.AverageUsagePercent);
        }

        Assert.True(memory.TotalBytes >= 0);
        Assert.True(swap.TotalBytes >= 0);
        Assert.True(storage.ApplicationDirectory.SizeBytes >= 0);
        Assert.True(storage.DownloadsDirectory.SizeBytes >= 0);
        Assert.InRange(storage.ApplicationDirectoryUsedDiskPercent, 0, 100);
        Assert.True(uptime > TimeSpan.Zero);
        Assert.True(currentProcessThreadCount > 0);
        Assert.True(systemThreadCount is null or >= 0);
        Assert.NotNull(network.IPv4Addresses);
        Assert.NotNull(network.IPv6Addresses);
    }

    private static ISystemInfoService CreateService()
    {
        var accessor = Substitute.For<IPanelSettingsAccessor>();
        accessor.Current.Returns(new XRayne.Contracts.Configurations.PanelOptions());
        var paths = new ProjectPathResolver(accessor);

        if (OperatingSystem.IsWindows())
        {
            return new WindowsSystemInfoService(paths);
        }

        if (OperatingSystem.IsLinux())
        {
            return new LinuxSystemInfoService(paths);
        }

        if (OperatingSystem.IsMacOS())
        {
            return new MacOsSystemInfoService(paths);
        }

        throw new PlatformNotSupportedException();
    }
}
