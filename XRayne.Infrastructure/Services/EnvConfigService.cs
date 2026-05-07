using System.Text;
using XRayne.Infrastructure.Values;

namespace XRayne.Infrastructure.Services;

public sealed class EnvConfigService : IEnvConfigService
{
    private readonly List<string> _lines;
    private readonly string _path;

    public EnvConfigService(string? savePath = null)
    {
        _path = savePath ?? PathProvider.Paths.EnvConfig;
        _lines = [];
    }

    private EnvConfigService(
        IEnumerable<string> lines,
        string? savePath = null)
    {
        _path = savePath ?? PathProvider.Paths.EnvConfig;
        _lines = lines.ToList();
    }

    public string? Get(string key)
    {
        foreach (var line in _lines)
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

    public void Set(string key, string value)
    {
        var updatedLine = $"{key}={Escape(value)}";
        for (var index = 0; index < _lines.Count; index++)
        {
            if (!TryParseLine(_lines[index], out var currentKey, out _))
            {
                continue;
            }

            if (string.Equals(currentKey, key, StringComparison.OrdinalIgnoreCase))
            {
                _lines[index] = updatedLine;
                return;
            }
        }

        _lines.Add(updatedLine);
    }

    public void Remove(string key)
    {
        for (var index = _lines.Count - 1; index >= 0; index--)
        {
            if (!TryParseLine(_lines[index], out var currentKey, out _))
            {
                continue;
            }

            if (string.Equals(currentKey, key, StringComparison.OrdinalIgnoreCase))
            {
                _lines.RemoveAt(index);
            }
        }
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        CheckAndCreateDirectory(_path);

        await File.WriteAllLinesAsync(_path, _lines, Encoding.UTF8, cancellationToken);
    }

    public static async Task<EnvConfigService> FromPath(
        string customPath,
        CancellationToken cancellationToken = default)
    {
        var path = customPath ?? PathProvider.Paths.EnvConfig;
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Directory or file not found", path);
        }

        var lines = await File.ReadAllLinesAsync(path, cancellationToken);

        return new EnvConfigService(lines, path);
    }

    public static async Task SetAndSaveAsync(
        string key,
        string value,
        string? customPath = null,
        CancellationToken cancellationToken = default)
    {
        var path = customPath ?? PathProvider.Paths.EnvConfig;
        var service = File.Exists(path)
            ? await FromPath(path, cancellationToken)
            : new EnvConfigService(path);

        service.Set(key, value);

        await service.SaveAsync(cancellationToken);
    }

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

    private static string Escape(string value)
    {
        if (value.Length == 0
            || !value.Any(character => char.IsWhiteSpace(character) || character is '#' or '=' or '"' or '\'' or '$'))
        {
            return value;
        }

        return $"'{value.Replace("'", "'\\''", StringComparison.Ordinal)}'";
    }

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

    private static void CheckAndCreateDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
