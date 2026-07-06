using Contracts.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace Test.Infrastructure;

public sealed class CacheStorageTests
{
    [Fact]
    public void Set_StoresValueByKey()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var store = new TestStorage(memoryCache);
        var value = new TestValue(42, "stored");

        store.Set(value);

        store.Get(42).Should().Be(value);
        store.TryGet(42, out var cached).Should().BeTrue();
        cached.Should().Be(value);
    }

    [Fact]
    public void GetAll_ReturnsStoredValues()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var store = new TestStorage(memoryCache);

        store.Set(new TestValue(1, "one"));
        store.Set(new TestValue(2, "two"));

        store.GetAll().Select(value => value.Id).Should().BeEquivalentTo([1, 2]);
    }

    [Fact]
    public void Remove_DeletesStoredValue()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var store = new TestStorage(memoryCache);
        store.Set(new TestValue(42, "stored"));

        store.Remove(42).Should().BeTrue();

        store.Get(42).Should().BeNull();
    }

    [Fact]
    public void Set_NotifiesListenersAfterValueIsStored()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var store = new TestStorage(memoryCache);
        TestValue? received = null;
        var listenerId = store.AddListener(value => received = store.Get(value.Id));
        var stored = new TestValue(42, "stored");

        store.Set(stored);

        received.Should().Be(stored);
        store.RemoveListener(listenerId).Should().BeTrue();
    }

    [Fact]
    public void RemoveListener_StopsNotifications()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var store = new TestStorage(memoryCache);
        var callCount = 0;
        var listenerId = store.AddListener(_ => callCount++);

        store.RemoveListener(listenerId);
        store.Set(new TestValue(42, "stored"));

        callCount.Should().Be(0);
    }

    private sealed record TestValue(int Id, string Name);

    private sealed class TestStorage(IMemoryCache cache)
        : CacheStorage<TestValue, int>(cache, "test-storage", value => value.Id);
}
