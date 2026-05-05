using Microsoft.Extensions.Configuration;

namespace XRayne.Cli.Services;

public sealed class XRayneEnvironmentConfigurationProvider(string path) : ConfigurationProvider
{
    public override void Load()
    {
        if (!File.Exists(path))
        {
            return;
        }

        var values = ReadEnvironmentFile(path);
        foreach (var (key, value) in values)
        {
            Data[key] = value;
            Data[key.Replace("__", ":", StringComparison.Ordinal)] = value;
        }

        AddDerivedConfiguration(values);
    }

    private void AddDerivedConfiguration(IReadOnlyDictionary<string, string> values)
    {
        if (values.TryGetValue("XRAYNE_DATA_FOLDER", out var dataFolder))
        {
            Data["XRayne:DataFolder"] = dataFolder;
            Data["Xray:Directory"] = dataFolder;
        }

        if (values.TryGetValue("XRAYNE_API_PREFIX", out var apiPrefix))
        {
            Data["PathBase"] = apiPrefix;
        }

        if (Data.ContainsKey("ConnectionStrings:Default"))
        {
            return;
        }

        if (!values.TryGetValue("POSTGRES_USER", out var user)
            || !values.TryGetValue("POSTGRES_PASSWORD", out var password)
            || !values.TryGetValue("POSTGRES_DB", out var database))
        {
            return;
        }

        var host = values.GetValueOrDefault("POSTGRES_HOST_CLI", "localhost");
        var port = values.GetValueOrDefault("POSTGRES_PORT", "5432");

        Data["ConnectionStrings:Default"] =
            $"Host={host};Port={port};Username={user};Password={password};Database={database}";
    }

    private static Dictionary<string, string> ReadEnvironmentFile(string filePath)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in File.ReadLines(filePath))
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.Length == 0 || trimmedLine.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = trimmedLine.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = trimmedLine[..separatorIndex].Trim();
            var value = trimmedLine[(separatorIndex + 1)..].Trim();
            values[key] = Unquote(value);
        }

        return values;
    }

    private static string Unquote(string value)
    {
        if (value.Length < 2)
        {
            return value;
        }

        var quote = value[0];
        if (quote is not ('"' or '\'') || value[^1] != quote)
        {
            return value;
        }

        return value[1..^1];
    }
}
