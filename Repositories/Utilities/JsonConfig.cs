using System.Text.Json;
using System.Text.Json.Nodes;
using Contracts.Values;

namespace Repositories.Utilities;

/// <summary>
/// Provides thread-safe helpers for reading and mutating the runtime JSON configuration file.
/// </summary>
public static class JsonConfig
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Sets or replaces a JSON value by a colon-delimited key and saves the file atomically.
    /// Missing parent sections are created automatically.
    /// </summary>
    /// <param name="key">Colon-delimited configuration key, for example <c>Kestrel:Endpoints:Http:Url</c>.</param>
    /// <param name="value">Value to serialize into the JSON document.</param>
    /// <param name="customPath">Optional config file path. Uses the default project config path when omitted.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task SetAsync<T>(
        string key,
        T value,
        string? customPath = null,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            customPath,
            root => Set(root, key, value),
            cancellationToken);
    }

    /// <summary>
    /// Updates a JSON value by passing the current value to a callback and saving the returned value atomically.
    /// The callback runs while the config file lock is held.
    /// </summary>
    /// <param name="key">Colon-delimited configuration key to update.</param>
    /// <param name="update">Callback that receives the current JSON node, or <c>null</c> when the key is missing.</param>
    /// <param name="customPath">Optional config file path. Uses the default project config path when omitted.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task UpdateValueAsync<T>(
        string key,
        Func<JsonNode?, T> update,
        string? customPath = null,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            customPath,
            root =>
            {
                var currentValue = Get(root, key);
                Set(root, key, update(currentValue));
            },
            cancellationToken);
    }

    /// <summary>
    /// Removes a JSON field or section by a colon-delimited key and saves the file atomically.
    /// Missing keys are ignored.
    /// </summary>
    /// <param name="key">Colon-delimited configuration key to remove.</param>
    /// <param name="customPath">Optional config file path. Uses the default project config path when omitted.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task RemoveAsync(
        string key,
        string? customPath = null,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            customPath,
            root => Remove(root, key),
            cancellationToken);
    }

    /// <summary>
    /// Removes a JSON field or section, then executes a synchronous callback before saving the default config file.
    /// The removal, callback, and save happen under the same file lock.
    /// </summary>
    /// <param name="key">Colon-delimited configuration key to remove.</param>
    /// <param name="update">Callback used for additional in-memory mutations after the removal.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task RemoveAsync(
        string key,
        Action<JsonObject> update,
        CancellationToken cancellationToken = default)
    {
        return RemoveAsync(key, null, update, cancellationToken);
    }

    /// <summary>
    /// Removes a JSON field or section, then executes an asynchronous callback before saving the default config file.
    /// The removal, callback, and save happen under the same file lock.
    /// </summary>
    /// <param name="key">Colon-delimited configuration key to remove.</param>
    /// <param name="update">Async callback used for additional in-memory mutations after the removal.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task RemoveAsync(
        string key,
        Func<JsonObject, Task> update,
        CancellationToken cancellationToken = default)
    {
        return RemoveAsync(key, null, update, cancellationToken);
    }

    /// <summary>
    /// Removes a JSON field or section, then executes a synchronous callback before saving the selected config file.
    /// The removal, callback, and save happen under the same file lock.
    /// </summary>
    /// <param name="key">Colon-delimited configuration key to remove.</param>
    /// <param name="customPath">Optional config file path. Uses the default project config path when omitted.</param>
    /// <param name="update">Callback used for additional in-memory mutations after the removal.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task RemoveAsync(
        string key,
        string? customPath,
        Action<JsonObject> update,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            customPath,
            root =>
            {
                Remove(root, key);
                update(root);
            },
            cancellationToken);
    }

    /// <summary>
    /// Removes a JSON field or section, then executes an asynchronous callback before saving the selected config file.
    /// The removal, callback, and save happen under the same file lock.
    /// </summary>
    /// <param name="key">Colon-delimited configuration key to remove.</param>
    /// <param name="customPath">Optional config file path. Uses the default project config path when omitted.</param>
    /// <param name="update">Async callback used for additional in-memory mutations after the removal.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task RemoveAsync(
        string key,
        string? customPath,
        Func<JsonObject, Task> update,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            customPath,
            async root =>
            {
                Remove(root, key);
                await update(root);
            },
            cancellationToken);
    }

    /// <summary>
    /// Loads the default config file, executes a synchronous mutation callback, and saves the file atomically.
    /// Missing files are treated as an empty JSON object.
    /// </summary>
    /// <param name="update">Callback that mutates the loaded root JSON object.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task UpdateAsync(
        Action<JsonObject> update,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(null, update, cancellationToken);
    }

    /// <summary>
    /// Loads the default config file, executes an asynchronous mutation callback, and saves the file atomically.
    /// Missing files are treated as an empty JSON object.
    /// </summary>
    /// <param name="update">Async callback that mutates the loaded root JSON object.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task UpdateAsync(
        Func<JsonObject, Task> update,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(null, update, cancellationToken);
    }

    /// <summary>
    /// Loads a config file, executes a synchronous mutation callback, and saves the file atomically.
    /// Missing files are treated as an empty JSON object.
    /// </summary>
    /// <param name="customPath">Optional config file path. Uses the default project config path when omitted.</param>
    /// <param name="update">Callback that mutates the loaded root JSON object.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static async Task UpdateAsync(
        string? customPath,
        Action<JsonObject> update,
        CancellationToken cancellationToken = default)
    {
        await UpdateAsync(
            customPath,
            root =>
            {
                update(root);
                return Task.CompletedTask;
            },
            cancellationToken);
    }

    /// <summary>
    /// Loads a config file, executes an asynchronous mutation callback, and saves the file atomically.
    /// Missing files are treated as an empty JSON object.
    /// </summary>
    /// <param name="customPath">Optional config file path. Uses the default project config path when omitted.</param>
    /// <param name="update">Async callback that mutates the loaded root JSON object.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static async Task UpdateAsync(
        string? customPath,
        Func<JsonObject, Task> update,
        CancellationToken cancellationToken = default)
    {
        var path = customPath ?? PathProvider.Paths.JsonConfig;
        using var _ = await ConfigFileLock.AcquireAsync(path, cancellationToken);
        var root = await LoadRootUnlockedAsync(path, cancellationToken);

        await update(root);

        await ConfigFileLock.WriteAllTextAtomicAsync(path, root.ToJsonString(Options), cancellationToken);
    }

    /// <summary>
    /// Loads an existing config file without saving it.
    /// </summary>
    /// <param name="customPath">Optional config file path. Uses the default project config path when omitted.</param>
    /// <param name="cancellationToken">Token used to cancel file IO.</param>
    /// <exception cref="FileNotFoundException">Thrown when the selected config file does not exist.</exception>
    public static async Task<JsonObject> LoadAsync(
        string? customPath = null,
        CancellationToken cancellationToken = default)
    {
        var path = customPath ?? PathProvider.Paths.JsonConfig;
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Directory or file not found", path);
        }

        return await LoadRootUnlockedAsync(path, cancellationToken);
    }

    /// <summary>
    /// Sets or replaces a JSON value on an in-memory root object.
    /// Missing parent sections are created automatically.
    /// </summary>
    /// <param name="root">Root JSON object to mutate.</param>
    /// <param name="key">Colon-delimited configuration key.</param>
    /// <param name="value">Value to serialize into the JSON object.</param>
    public static void Set<T>(
        JsonObject root,
        string key,
        T value)
    {
        var segments = SplitKey(key);
        var current = root;

        for (var index = 0; index < segments.Length - 1; index++)
        {
            var segment = segments[index];
            if (!current.TryGetPropertyValue(segment, out var child) || child is not JsonObject childObject)
            {
                childObject = new JsonObject();
                current[segment] = childObject;
            }

            current = childObject;
        }

        current[segments[^1]] = JsonSerializer.SerializeToNode(value, Options);
    }

    /// <summary>
    /// Removes a JSON field or section from an in-memory root object.
    /// Missing keys are ignored.
    /// </summary>
    /// <param name="root">Root JSON object to mutate.</param>
    /// <param name="key">Colon-delimited configuration key to remove.</param>
    public static void Remove(
        JsonObject root,
        string key)
    {
        var segments = SplitKey(key);
        var current = root;

        for (var index = 0; index < segments.Length - 1; index++)
        {
            var segment = segments[index];
            if (!current.TryGetPropertyValue(segment, out var child) || child is not JsonObject childObject)
            {
                return;
            }

            current = childObject;
        }

        current.Remove(segments[^1]);
    }

    /// <summary>
    /// Gets a JSON node from an in-memory root object by a colon-delimited key.
    /// </summary>
    private static JsonNode? Get(
        JsonObject root,
        string key)
    {
        var segments = SplitKey(key);
        JsonNode? current = root;

        foreach (var segment in segments)
        {
            if (current is not JsonObject currentObject)
            {
                return null;
            }

            if (!currentObject.TryGetPropertyValue(segment, out current))
            {
                return null;
            }
        }

        return current;
    }

    /// <summary>
    /// Loads a JSON root object without acquiring a file lock.
    /// </summary>
    private static async Task<JsonObject> LoadRootUnlockedAsync(
        string path,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return new JsonObject();
        }

        try
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken);

            return JsonNode.Parse(json) as JsonObject ?? new JsonObject();
        }
        catch
        {
            return new JsonObject();
        }
    }

    /// <summary>
    /// Splits and validates a colon-delimited JSON configuration key.
    /// </summary>
    private static string[] SplitKey(string key)
    {
        var segments = key.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            throw new ArgumentException("JSON config key cannot be empty.", nameof(key));
        }

        return segments;
    }
}
