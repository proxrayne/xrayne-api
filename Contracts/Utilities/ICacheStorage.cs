namespace Contracts.Utilities;

/// <summary>
/// Stores keyed objects and notifies listeners when values are added.
/// </summary>
public interface ICacheStorage<TValue, TKey>
    where TValue : class
    where TKey : notnull
{
    /// <summary>
    /// Gets a stored value by key.
    /// </summary>
    TValue? Get(TKey key);

    /// <summary>
    /// Tries to get a stored value by key.
    /// </summary>
    bool TryGet(TKey key, out TValue? value);

    /// <summary>
    /// Gets all stored values.
    /// </summary>
    IReadOnlyCollection<TValue> GetAll();

    /// <summary>
    /// Stores a value and notifies listeners.
    /// </summary>
    void Set(TValue value);

    /// <summary>
    /// Removes a stored value.
    /// </summary>
    bool Remove(TKey key);

    /// <summary>
    /// Removes all stored values.
    /// </summary>
    void Clear();

    /// <summary>
    /// Adds a listener that is called after a value is stored.
    /// </summary>
    Guid AddListener(Action<TValue> onSet);

    /// <summary>
    /// Removes a listener.
    /// </summary>
    bool RemoveListener(Guid id);
}
