using System.Text.Json.Nodes;
using XRayne.Infrastructure.Services;

namespace XRayne.Test.Infrastructure;

public sealed class JsonConfigServiceTests
{
    [Fact]
    public async Task SaveAsync_WritesNestedConfiguration()
    {
        using var workspace = new TestWorkspace();
        var configPath = workspace.GetPath("nested", "config.json");
        var service = new JsonConfigService(configPath);

        service.Set("Kestrel:Endpoints:Https:Url", "https://+:5000");
        service.Set("Runtime:SchemaVersion", 1);

        await service.SaveAsync();

        var root = await ReadJsonAsync(configPath);
        Assert.Equal("https://+:5000", root["Kestrel"]?["Endpoints"]?["Https"]?["Url"]?.GetValue<string>());
        Assert.Equal(1, root["Runtime"]?["SchemaVersion"]?.GetValue<int>());
    }

    [Fact]
    public async Task FromPath_LoadsExistingJsonAndRemoveDeletesNestedValue()
    {
        using var workspace = new TestWorkspace();
        var configPath = workspace.GetPath("config.json");
        await File.WriteAllTextAsync(
            configPath,
            """
            {
              "Kestrel": {
                "Endpoints": {
                  "Http": { "Url": "http://+:8080" },
                  "Https": { "Url": "https://+:8443" }
                }
              }
            }
            """);

        var service = await JsonConfigService.FromPath(configPath);
        service.Remove("Kestrel:Endpoints:Http");
        service.Set("PathBase", "/panel");

        await service.SaveAsync();

        var root = await ReadJsonAsync(configPath);
        Assert.Null(root["Kestrel"]?["Endpoints"]?["Http"]);
        Assert.Equal("https://+:8443", root["Kestrel"]?["Endpoints"]?["Https"]?["Url"]?.GetValue<string>());
        Assert.Equal("/panel", root["PathBase"]?.GetValue<string>());
    }

    [Fact]
    public async Task SetAndSaveAsync_UpdatesExistingFile()
    {
        using var workspace = new TestWorkspace();
        var configPath = workspace.GetPath("config.json");
        await File.WriteAllTextAsync(configPath, """{"Runtime":{"SchemaVersion":0}}""");

        await JsonConfigService.SetAndSaveAsync("Runtime:SchemaVersion", 1, configPath);

        var root = await ReadJsonAsync(configPath);
        Assert.Equal(1, root["Runtime"]?["SchemaVersion"]?.GetValue<int>());
    }

    private static async Task<JsonNode> ReadJsonAsync(string path)
    {
        var json = await File.ReadAllTextAsync(path);

        return JsonNode.Parse(json) ?? throw new InvalidOperationException("JSON was empty.");
    }
}
