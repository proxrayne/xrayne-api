using System.Text.Json.Nodes;
using XRayne.Cli.Values;
using XRayne.Infrastructure.Services;
using XRayne.Infrastructure.Values;

namespace XRayne.Cli.Services.RuntimeMigrations;

internal static class RuntimeMigrationFileHelpers
{
    public static async Task<JsonConfigService> LoadConfigAsync(
        ProjectPaths paths,
        CancellationToken cancellationToken)
    {
        return File.Exists(paths.JsonConfig)
            ? await JsonConfigService.FromPath(paths.JsonConfig, cancellationToken)
            : new JsonConfigService(paths.JsonConfig);
    }

    public static async Task<int> ReadSchemaVersionAsync(
        ProjectPaths paths,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(paths.JsonConfig))
        {
            return 0;
        }

        try
        {
            var json = await File.ReadAllTextAsync(paths.JsonConfig, cancellationToken);
            var root = JsonNode.Parse(json);
            var version = root?["Runtime"]?["SchemaVersion"]?.GetValue<int>();

            return version ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    public static async Task SetSchemaVersionAsync(
        ProjectPaths paths,
        int schemaVersion,
        CancellationToken cancellationToken)
    {
        var config = await LoadConfigAsync(paths, cancellationToken);
        config.Set("Runtime:SchemaVersion", schemaVersion);

        await config.SaveAsync(cancellationToken);
    }

    public static async Task<string> GetApiPortAsync(
        ProjectPaths paths,
        CancellationToken cancellationToken)
    {
        var env = File.Exists(paths.EnvConfig)
            ? await EnvConfigService.FromPath(paths.EnvConfig, cancellationToken)
            : new EnvConfigService(paths.EnvConfig);
        var apiPort = env.Get("API_PORT");

        return !string.IsNullOrWhiteSpace(apiPort)
            ? apiPort
            : CliDefaults.DefaultApiPort.ToString();
    }

    public static bool HasCertificate(ProjectPaths paths)
    {
        if (!File.Exists(paths.JsonConfig))
        {
            return false;
        }

        try
        {
            var json = File.ReadAllText(paths.JsonConfig);
            var root = JsonNode.Parse(json);

            return !string.IsNullOrWhiteSpace(root?["Certificate"]?["CertName"]?.GetValue<string>())
                || !string.IsNullOrWhiteSpace(root?["Kestrel"]?["Endpoints"]?["Https"]?["Url"]?.GetValue<string>());
        }
        catch
        {
            return false;
        }
    }

    public static async Task SetEnvValueAsync(
        string envPath,
        string key,
        string value,
        CancellationToken cancellationToken)
    {
        var env = File.Exists(envPath)
            ? await EnvConfigService.FromPath(envPath, cancellationToken)
            : new EnvConfigService(envPath);
        env.Set(key, value);

        await env.SaveAsync(cancellationToken);
    }
}
