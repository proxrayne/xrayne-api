using Microsoft.Extensions.Caching.Memory;

namespace Contracts.Utilities;

/// <summary>
/// Stores keyed objects in memory cache and notifies listeners when values are added.
/// </summary>
public abstract class CacheStorage<TValue, TKey>(
    IMemoryCache cache,
    string storageKey,
    Func<TValue, TKey> getEntityKey)
    : ICacheStorage<TValue, TKey>
    where TValue : class
    where TKey : notnull
{
    /// <summary>
    /// Gets the cache lifetime for the backing object dictionary.
    /// </summary>
    public TimeSpan CacheTtl { get; init; } = TimeSpan.FromDays(7);

    private readonly Lock storageLock = new();
    private readonly Dictionary<Guid, Listener<TValue>> listeners = new();

    /// <summary>
    /// Gets a stored value by key.
    /// </summary>
    public TValue? Get(TKey key)
    {
        lock (storageLock)
        {
            return GetValues().GetValueOrDefault(key);
        }
    }

    /// <summary>
    /// Tries to get a stored value by key.
    /// </summary>
    public bool TryGet(TKey key, out TValue? value)
    {
        lock (storageLock)
        {
            return GetValues().TryGetValue(key, out value);
        }
    }

    /// <summary>
    /// Gets all stored values.
    /// </summary>
    public IReadOnlyCollection<TValue> GetAll()
    {
        lock (storageLock)
        {
            return GetValues().Values.ToList();
        }
    }

    /// <summary>
    /// Stores a value and notifies listeners.
    /// </summary>
    public void Set(TValue value)
    {
        Listener<TValue>[] listenersSnapshot;
        lock (storageLock)
        {
            var values = GetValues();
            values[getEntityKey(value)] = value;
            cache.Set(storageKey, values, CacheTtl);
            listenersSnapshot = listeners.Values.ToArray();
        }

        foreach (var listener in listenersSnapshot)
        {
            listener.OnSet(value);
        }
    }

    /// <summary>
    /// Removes a stored value.
    /// </summary>
    public bool Remove(TKey key)
    {
        lock (storageLock)
        {
            var values = GetValues();
            if (!values.Remove(key))
            {
                return false;
            }

            cache.Set(storageKey, values, CacheTtl);
            return true;
        }
    }

    /// <summary>
    /// Removes all stored values.
    /// </summary>
    public void Clear()
    {
        lock (storageLock)
        {
            cache.Remove(storageKey);
        }
    }

    /// <summary>
    /// Adds a listener that is called after a value is stored.
    /// </summary>
    public Guid AddListener(Action<TValue> onSet)
    {
        ArgumentNullException.ThrowIfNull(onSet);

        lock (storageLock)
        {
            var id = Guid.NewGuid();
            listeners[id] = new Listener<TValue>(id, onSet);

            return id;
        }
    }

    /// <summary>
    /// Removes a listener.
    /// </summary>
    public bool RemoveListener(Guid id)
    {
        lock (storageLock)
        {
            return listeners.Remove(id);
        }
    }

    private Dictionary<TKey, TValue> GetValues()
    {
        return cache.Get<Dictionary<TKey, TValue>>(storageKey) ?? new Dictionary<TKey, TValue>();
    }
}

/// <summary>
/// Describes a cache storage listener.
/// </summary>
public sealed record Listener<TValue>(Guid Id, Action<TValue> OnSet) where TValue : class;
