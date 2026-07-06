using Contracts.Enums;
using Contracts.Models;
using Microsoft.Extensions.Caching.Memory;
using Data.Implementations;

namespace Test.Infrastructure;

public sealed class RemoteNodeCoreStateStoreTests
{
    [Fact]
    public void Set_StoresCoreRuntimeState()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var store = new RemoteNodeCoreStateStore(memoryCache);
        var startedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        var uptime = TimeSpan.FromMinutes(5);
        var state = new RemoteNodeCoreState(
            42,
            true,
            true,
            "25.7.1",
            CoreStatus.Started,
            startedAt,
            uptime);

        store.Set(state);

        var cached = store.Get(42);
        cached.Should().Be(state);
        cached!.IsInstalled.Should().BeTrue();
        cached.IsRunning.Should().BeTrue();
        cached.Version.Should().Be("25.7.1");
        cached.Status.Should().Be(CoreStatus.Started);
        cached.StartedAt.Should().Be(startedAt);
        cached.Uptime.Should().Be(uptime);
    }
}
