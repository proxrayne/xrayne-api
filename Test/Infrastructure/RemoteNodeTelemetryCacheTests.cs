using Microsoft.Extensions.Caching.Memory;
using Infrastructure.Services;
using RemoteNode.Models;

namespace Test.Infrastructure;

public sealed class RemoteNodeTelemetryCacheTests
{
    [Fact]
    public void Get_ReturnsNull_WhenSnapshotIsMissing()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new RemoteNodeTelemetryCache(memoryCache);

        var snapshot = cache.Get(42);

        snapshot.Should().BeNull();
    }

    [Fact]
    public void Set_StoresHeartbeatSnapshotByNodeId()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new RemoteNodeTelemetryCache(memoryCache);
        var telemetry = new NodePingResponse(
            "1.2.3",
            "Production",
            TimeSpan.FromMinutes(10),
            new NodeCoreStatus(true, true, "25.7.1", "started"));
        var snapshot = new RemoteNodeConnectionSnapshot(
            42,
            RemoteNodeConnectionState.Connected,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            0,
            null,
            telemetry);

        cache.Set(snapshot);

        var cached = cache.Get(42);
        cached.Should().Be(snapshot);
        cached!.Telemetry.Should().BeSameAs(telemetry);
    }

    [Theory]
    [InlineData(RemoteNodeConnectionState.Disconnected, "Remote node stream is stopped.")]
    [InlineData(RemoteNodeConnectionState.Error, "Connection failed.")]
    public void Set_StoresConnectionLifecycleSnapshot(
        RemoteNodeConnectionState state,
        string message)
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cache = new RemoteNodeTelemetryCache(memoryCache);
        var snapshot = new RemoteNodeConnectionSnapshot(
            42,
            state,
            DateTimeOffset.UtcNow,
            null,
            null,
            2,
            message,
            null);

        cache.Set(snapshot);

        var cached = cache.Get(42);
        cached.Should().Be(snapshot);
        cached!.State.Should().Be(state);
        cached.Message.Should().Be(message);
    }
}
