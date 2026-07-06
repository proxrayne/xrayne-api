using Contracts.Enums;
using Contracts.Models;
using Microsoft.Extensions.Caching.Memory;
using Repositories.Implementations;

namespace Test.Infrastructure;

public sealed class NodeConnectionStateStoreTests
{
    [Fact]
    public void Set_StoresConnectionStatusApiVersionAndUptime()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var store = new NodeConnectionStateStore(memoryCache);
        var uptime = DateTimeOffset.UtcNow.AddMinutes(-5);
        var state = new NodeConnectionState(
            42,
            NodeConnectionStatus.Connected,
            "1.2.3",
            uptime);

        store.Set(state);

        var cached = store.Get(42);
        cached.Should().Be(state);
        cached!.Status.Should().Be(NodeConnectionStatus.Connected);
        cached.ApiVersion.Should().Be("1.2.3");
        cached.Uptime.Should().Be(uptime);
    }
}
