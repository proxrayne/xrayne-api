
using XRayne.Contracts.Values;

namespace XRayne.Infrastructure.Utilities;

/// <summary>
/// Provides thread-safe helpers for reading and mutating project <c>.env</c> files.
/// </summary>
public static class EnvConfig
{
    /// <summary>
    /// Reads a value from an existing <c>.env</c> file by key.
    /// </summary>
    /// <param name="key">Environment variable key to read. Matching is case-insensitive.</param>
    /// <param name="customPath">Optional <c>.env</c> file path. Uses the default project <c>.env</c> path when omitted.</param>
    /// <param name="cancellationToken">Token used to cancel file IO.</param>
    /// <exception cref="FileNotFoundException">Thrown when the selected <c>.env</c> file does not exist.</exception>
    public static async Task<string?> GetAsync(
        string key,
        string? customPath = null,
        CancellationToken cancellationToken = default)
    {
        var lines = await LoadAsync(customPath, cancellationToken);

        return Get(lines, key);
    }

    /// <summary>
    /// Sets or replaces a value in a <c>.env</c> file and saves the file atomically.
    /// Missing files are treated as empty files.
    /// </summary>
    /// <param name="key">Environment variable key to set. Existing keys are matched case-insensitively.</param>
    /// <param name="value">Environment variable value. Values that need quoting are escaped automatically.</param>
    /// <param name="customPath">Optional <c>.env</c> file path. Uses the default project <c>.env</c> path when omitted.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task SetAsync(
        string key,
        string value,
        string? customPath = null,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            customPath,
            lines => Set(lines, key, value),
            cancellationToken);
    }

    /// <summary>
    /// Updates a value by passing the current value to a callback and saving the returned value atomically.
    /// The callback runs while the <c>.env</c> file lock is held.
    /// </summary>
    /// <param name="key">Environment variable key to update. Matching is case-insensitive.</param>
    /// <param name="update">Callback that receives the current value, or <c>null</c> when the key is missing.</param>
    /// <param name="customPath">Optional <c>.env</c> file path. Uses the default project <c>.env</c> path when omitted.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task UpdateValueAsync(
        string key,
        Func<string?, string> update,
        string? customPath = null,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            customPath,
            lines =>
            {
                var currentValue = Get(lines, key);
                Set(lines, key, update(currentValue));
            },
            cancellationToken);
    }

    /// <summary>
    /// Removes all matching keys from a <c>.env</c> file and saves the file atomically.
    /// Missing keys are ignored.
    /// </summary>
    /// <param name="key">Environment variable key to remove. Matching is case-insensitive.</param>
    /// <param name="customPath">Optional <c>.env</c> file path. Uses the default project <c>.env</c> path when omitted.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task RemoveAsync(
        string key,
        string? customPath = null,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            customPath,
            lines => Remove(lines, key),
            cancellationToken);
    }

    /// <summary>
    /// Removes all matching keys, then executes a synchronous callback before saving the default <c>.env</c> file.
    /// The removal, callback, and save happen under the same file lock.
    /// </summary>
    /// <param name="key">Environment variable key to remove. Matching is case-insensitive.</param>
    /// <param name="update">Callback used for additional in-memory line mutations after the removal.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task RemoveAsync(
        string key,
        Action<List<string>> update,
        CancellationToken cancellationToken = default)
    {
        return RemoveAsync(key, null, update, cancellationToken);
    }

    /// <summary>
    /// Removes all matching keys, then executes an asynchronous callback before saving the default <c>.env</c> file.
    /// The removal, callback, and save happen under the same file lock.
    /// </summary>
    /// <param name="key">Environment variable key to remove. Matching is case-insensitive.</param>
    /// <param name="update">Async callback used for additional in-memory line mutations after the removal.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task RemoveAsync(
        string key,
        Func<List<string>, Task> update,
        CancellationToken cancellationToken = default)
    {
        return RemoveAsync(key, null, update, cancellationToken);
    }

    /// <summary>
    /// Removes all matching keys, then executes a synchronous callback before saving the selected <c>.env</c> file.
    /// The removal, callback, and save happen under the same file lock.
    /// </summary>
    /// <param name="key">Environment variable key to remove. Matching is case-insensitive.</param>
    /// <param name="customPath">Optional <c>.env</c> file path. Uses the default project <c>.env</c> path when omitted.</param>
    /// <param name="update">Callback used for additional in-memory line mutations after the removal.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task RemoveAsync(
        string key,
        string? customPath,
        Action<List<string>> update,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            customPath,
            lines =>
            {
                Remove(lines, key);
                update(lines);
            },
            cancellationToken);
    }

    /// <summary>
    /// Removes all matching keys, then executes an asynchronous callback before saving the selected <c>.env</c> file.
    /// The removal, callback, and save happen under the same file lock.
    /// </summary>
    /// <param name="key">Environment variable key to remove. Matching is case-insensitive.</param>
    /// <param name="customPath">Optional <c>.env</c> file path. Uses the default project <c>.env</c> path when omitted.</param>
    /// <param name="update">Async callback used for additional in-memory line mutations after the removal.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task RemoveAsync(
        string key,
        string? customPath,
        Func<List<string>, Task> update,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(
            customPath,
            async lines =>
            {
                Remove(lines, key);
                await update(lines);
            },
            cancellationToken);
    }

    /// <summary>
    /// Loads the default <c>.env</c> file, executes a synchronous mutation callback, and saves the file atomically.
    /// Missing files are treated as empty files.
    /// </summary>
    /// <param name="update">Callback that mutates the loaded line collection.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task UpdateAsync(
        Action<List<string>> update,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(null, update, cancellationToken);
    }

    /// <summary>
    /// Loads the default <c>.env</c> file, executes an asynchronous mutation callback, and saves the file atomically.
    /// Missing files are treated as empty files.
    /// </summary>
    /// <param name="update">Async callback that mutates the loaded line collection.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static Task UpdateAsync(
        Func<List<string>, Task> update,
        CancellationToken cancellationToken = default)
    {
        return UpdateAsync(null, update, cancellationToken);
    }

    /// <summary>
    /// Loads a <c>.env</c> file, executes a synchronous mutation callback, and saves the file atomically.
    /// Missing files are treated as empty files.
    /// </summary>
    /// <param name="customPath">Optional <c>.env</c> file path. Uses the default project <c>.env</c> path when omitted.</param>
    /// <param name="update">Callback that mutates the loaded line collection.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static async Task UpdateAsync(
        string? customPath,
        Action<List<string>> update,
        CancellationToken cancellationToken = default)
    {
        await UpdateAsync(
            customPath,
            lines =>
            {
                update(lines);
                return Task.CompletedTask;
            },
            cancellationToken);
    }

    /// <summary>
    /// Loads a <c>.env</c> file, executes an asynchronous mutation callback, and saves the file atomically.
    /// Missing files are treated as empty files.
    /// </summary>
    /// <param name="customPath">Optional <c>.env</c> file path. Uses the default project <c>.env</c> path when omitted.</param>
    /// <param name="update">Async callback that mutates the loaded line collection.</param>
    /// <param name="cancellationToken">Token used to cancel file IO or lock acquisition.</param>
    public static async Task UpdateAsync(
        string? customPath,
        Func<List<string>, Task> update,
        CancellationToken cancellationToken = default)
    {
        var path = customPath ?? PathProvider.Paths.EnvConfig;
        using var _ = await ConfigFileLock.AcquireAsync(path, cancellationToken);
        var lines = await LoadLinesUnlockedAsync(path, cancellationToken);

        await update(lines);

        await ConfigFileLock.WriteAllLinesAtomicAsync(path, lines, cancellationToken);
    }

    /// <summary>
    /// Loads an existing <c>.env</c> file without saving it.
    /// </summary>
    /// <param name="customPath">Optional <c>.env</c> file path. Uses the default project <c>.env</c> path when omitted.</param>
    /// <param name="cancellationToken">Token used to cancel file IO.</param>
    /// <exception cref="FileNotFoundException">Thrown when the selected <c>.env</c> file does not exist.</exception>
    public static async Task<List<string>> LoadAsync(
        string? customPath = null,
        CancellationToken cancellationToken = default)
    {
        var path = customPath ?? PathProvider.Paths.EnvConfig;
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Directory or file not found", path);
        }

        return await LoadLinesUnlockedAsync(path, cancellationToken);
    }

    /// <summary>
    /// Reads a value from an in-memory <c>.env</c> line collection.
    /// </summary>
    /// <param name="lines">Line collection to read from.</param>
    /// <param name="key">Environment variable key to read. Matching is case-insensitive.</param>
    public static string? Get(
        IEnumerable<string> lines,
        string key)
    {
        foreach (var line in lines)
        {
            if (!TryParseLine(line, out var currentKey, out var currentValue))
            {
                continue;
            }

            if (string.Equals(currentKey, key, StringComparison.OrdinalIgnoreCase))
            {
                return currentValue;
            }
        }

        return null;
    }

    /// <summary>
    /// Sets or replaces a value in an in-memory <c>.env</c> line collection.
    /// Comments and unrelated lines are preserved.
    /// </summary>
    /// <param name="lines">Line collection to mutate.</param>
    /// <param name="key">Environment variable key to set. Existing keys are matched case-insensitively.</param>
    /// <param name="value">Environment variable value. Values that need quoting are escaped automatically.</param>
    public static void Set(
        List<string> lines,
        string key,
        string value)
    {
        var updatedLine = $"{key}={Escape(value)}";
        for (var index = 0; index < lines.Count; index++)
        {
            if (!TryParseLine(lines[index], out var currentKey, out _))
            {
                continue;
            }

            if (string.Equals(currentKey, key, StringComparison.OrdinalIgnoreCase))
            {
                lines[index] = updatedLine;
                return;
            }
        }

        lines.Add(updatedLine);
    }

    /// <summary>
    /// Removes all matching keys from an in-memory <c>.env</c> line collection.
    /// Comments and unrelated lines are preserved.
    /// </summary>
    /// <param name="lines">Line collection to mutate.</param>
    /// <param name="key">Environment variable key to remove. Matching is case-insensitive.</param>
    public static void Remove(
        List<string> lines,
        string key)
    {
        for (var index = lines.Count - 1; index >= 0; index--)
        {
            if (!TryParseLine(lines[index], out var currentKey, out _))
            {
                continue;
            }

            if (string.Equals(currentKey, key, StringComparison.OrdinalIgnoreCase))
            {
                lines.RemoveAt(index);
            }
        }
    }

    /// <summary>
    /// Parses a single <c>.env</c> assignment line into a key and unescaped value.
    /// </summary>
    private static bool TryParseLine(
        string line,
        out string key,
        out string value)
    {
        key = string.Empty;
        value = string.Empty;

        if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
        {
            return false;
        }

        var parts = line.Split('=', 2);
        if (parts.Length != 2)
        {
            return false;
        }

        key = parts[0].Trim();
        value = Unescape(parts[1].Trim());

        return key.Length > 0;
    }

    /// <summary>
    /// Escapes a value when it cannot be safely written as a plain <c>.env</c> value.
    /// </summary>
    private static string Escape(string value)
    {
        if (value.Length == 0
            || !value.Any(character => char.IsWhiteSpace(character) || character is '#' or '=' or '"' or '\'' or '$'))
        {
            return value;
        }

        return $"'{value.Replace("'", "'\\''", StringComparison.Ordinal)}'";
    }

    /// <summary>
    /// Removes simple single-quote or double-quote wrapping from a parsed <c>.env</c> value.
    /// </summary>
    private static string Unescape(string value)
    {
        if (value.Length < 2)
        {
            return value;
        }

        if (value[0] == '\'' && value[^1] == '\'')
        {
            return value[1..^1].Replace("'\\''", "'", StringComparison.Ordinal);
        }

        if (value[0] == '"' && value[^1] == '"')
        {
            return value[1..^1]
                .Replace("\\\"", "\"", StringComparison.Ordinal)
                .Replace("\\\\", "\\", StringComparison.Ordinal);
        }

        return value;
    }

    /// <summary>
    /// Loads <c>.env</c> lines without acquiring a file lock.
    /// </summary>
    private static async Task<List<string>> LoadLinesUnlockedAsync(
        string path,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return [];
        }

        var lines = await File.ReadAllLinesAsync(path, cancellationToken);

        return lines.ToList();
    }
}
