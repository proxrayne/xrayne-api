using System.Text.Json;
using System.Text.Json.Nodes;
using XRayne.Infrastructure.Values;

namespace XRayne.Infrastructure.Services;

public sealed class JsonConfigService : IJsonConfigService
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    private JsonNode _rootNode = new JsonObject();
    private readonly string _path;

    public JsonConfigService(JsonNode node, string? savePath = null)
    {
        _path = savePath ?? PathProvider.Paths.JsonConfig;
        _rootNode = node;
    }

    public JsonConfigService(string? savePath = null)
    {
        _path = savePath ?? PathProvider.Paths.JsonConfig;
        _rootNode = new JsonObject();
    }

    public void Set<T>(string key, T value)
    {
        Set(_rootNode, key, value);
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        CheckAndCreateDirectory(_path);

        await File.WriteAllTextAsync(_path, _rootNode.ToJsonString(_options), cancellationToken: ct);
    }

    public static async Task<JsonConfigService> FromPath(string customPath, CancellationToken ct = default)
    {
        var path = customPath ?? PathProvider.Paths.JsonConfig;;
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Directory or file not found");
        }

        JsonNode node = new JsonObject();

        try
        {
            var json = await File.ReadAllTextAsync(path, cancellationToken: ct);

            node = JsonNode.Parse(json) ?? new JsonObject();
        }
        catch { }

        return new JsonConfigService(node, path);
    }

    public static async Task SetAndSaveAsync<T>(string key, T value, string? customPath = null, CancellationToken ct = default)
    {
        var service = await JsonConfigService.FromPath(customPath ?? PathProvider.Paths.JsonConfig);

        service.Set(key, value);

        await service.SaveAsync(ct);
    }

    private static void Set<T>(JsonNode node, string key, T value)
    {
        var segments = key.Split(':');
        var current = node.AsObject();

        for (int i = 0; i < segments.Length - 1; i++)
        {
            string segment = segments[i];
            if (!current.ContainsKey(segment) || current[segment] is not JsonObject)
            {
                current[segment] = new JsonObject();
            }

            current = current[segment]!.AsObject();
        }

        current[segments[^1]] = JsonSerializer.SerializeToNode(value, _options);
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