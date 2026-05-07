using System.Text.Json.Nodes;
using XRayne.Infrastructure.Utilities;

namespace XRayne.Test.Infrastructure;

public sealed class JsonConfigTests
{
    [Fact]
    public async Task SetAsync_WritesNestedConfiguration()
    {
        using var workspace = new TestWorkspace();
        var configPath = workspace.GetPath("nested", "config.json");

        await JsonConfig.SetAsync("Kestrel:Endpoints:Https:Url", "https://+:5000", configPath);
        await JsonConfig.SetAsync("Runtime:SchemaVersion", 1, configPath);

        var root = await ReadJsonAsync(configPath);
        Assert.Equal("https://+:5000", root["Kestrel"]?["Endpoints"]?["Https"]?["Url"]?.GetValue<string>());
        Assert.Equal(1, root["Runtime"]?["SchemaVersion"]?.GetValue<int>());
    }

    [Fact]
    public async Task UpdateAsync_LoadsExistingJsonAndRemoveDeletesNestedValue()
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

        await JsonConfig.UpdateAsync(
            configPath,
            config =>
            {
                JsonConfig.Remove(config, "Kestrel:Endpoints:Http");
                JsonConfig.Set(config, "PathBase", "/panel");
            });

        var root = await ReadJsonAsync(configPath);
        Assert.Null(root["Kestrel"]?["Endpoints"]?["Http"]);
        Assert.Equal("https://+:8443", root["Kestrel"]?["Endpoints"]?["Https"]?["Url"]?.GetValue<string>());
        Assert.Equal("/panel", root["PathBase"]?.GetValue<string>());
    }

    [Fact]
    public async Task SetAsync_UpdatesExistingFile()
    {
        using var workspace = new TestWorkspace();
        var configPath = workspace.GetPath("config.json");
        await File.WriteAllTextAsync(configPath, """{"Runtime":{"SchemaVersion":0}}""");

        await JsonConfig.SetAsync("Runtime:SchemaVersion", 1, configPath);

        var root = await ReadJsonAsync(configPath);
        Assert.Equal(1, root["Runtime"]?["SchemaVersion"]?.GetValue<int>());
    }

    [Fact]
    public async Task RemoveAsync_CanRemoveSection()
    {
        using var workspace = new TestWorkspace();
        var configPath = workspace.GetPath("config.json");
        await File.WriteAllTextAsync(configPath, """{"Kestrel":{"Endpoints":{"Http":{"Url":"http://+:8080"}}}}""");

        await JsonConfig.RemoveAsync("Kestrel:Endpoints:Http", configPath);

        var root = await ReadJsonAsync(configPath);
        Assert.Null(root["Kestrel"]?["Endpoints"]?["Http"]);
    }

    [Fact]
    public async Task RemoveAsync_CanRunCallbackAfterRemoval()
    {
        using var workspace = new TestWorkspace();
        var configPath = workspace.GetPath("config.json");
        await File.WriteAllTextAsync(configPath, """{"Kestrel":{"Endpoints":{"Http":{"Url":"http://+:8080"}}}}""");

        await JsonConfig.RemoveAsync(
            "Kestrel:Endpoints:Http",
            configPath,
            config => JsonConfig.Set(config, "Runtime:SchemaVersion", 1));

        var root = await ReadJsonAsync(configPath);
        Assert.Null(root["Kestrel"]?["Endpoints"]?["Http"]);
        Assert.Equal(1, root["Runtime"]?["SchemaVersion"]?.GetValue<int>());
    }

    [Fact]
    public async Task RemoveAsync_CanRunAsyncCallbackAfterRemoval()
    {
        using var workspace = new TestWorkspace();
        var configPath = workspace.GetPath("config.json");
        await File.WriteAllTextAsync(configPath, """{"Kestrel":{"Endpoints":{"Http":{"Url":"http://+:8080"}}}}""");

        await JsonConfig.RemoveAsync(
            "Kestrel:Endpoints:Http",
            configPath,
            async config =>
            {
                await Task.Yield();
                JsonConfig.Set(config, "Async:Value", true);
            });

        var root = await ReadJsonAsync(configPath);
        Assert.Null(root["Kestrel"]?["Endpoints"]?["Http"]);
        Assert.True(root["Async"]?["Value"]?.GetValue<bool>());
    }

    [Fact]
    public async Task UpdateValueAsync_UsesCurrentValue()
    {
        using var workspace = new TestWorkspace();
        var configPath = workspace.GetPath("config.json");
        await File.WriteAllTextAsync(configPath, """{"Runtime":{"SchemaVersion":1}}""");

        await JsonConfig.UpdateValueAsync(
            "Runtime:SchemaVersion",
            current => current?.GetValue<int>() + 1 ?? 1,
            configPath);

        var root = await ReadJsonAsync(configPath);
        Assert.Equal(2, root["Runtime"]?["SchemaVersion"]?.GetValue<int>());
    }

    [Fact]
    public async Task UpdateAsync_PreservesConcurrentUpdates()
    {
        using var workspace = new TestWorkspace();
        var configPath = workspace.GetPath("config.json");

        await Task.WhenAll(
            JsonConfig.UpdateAsync(configPath, config => JsonConfig.Set(config, "First:Value", "one")),
            JsonConfig.UpdateAsync(configPath, config => JsonConfig.Set(config, "Second:Value", "two")));

        var root = await ReadJsonAsync(configPath);
        Assert.Equal("one", root["First"]?["Value"]?.GetValue<string>());
        Assert.Equal("two", root["Second"]?["Value"]?.GetValue<string>());
        Assert.Empty(Directory.GetFiles(workspace.Root, "*.tmp"));
    }

    [Fact]
    public async Task UpdateAsync_CanRunAsyncCallback()
    {
        using var workspace = new TestWorkspace();
        var configPath = workspace.GetPath("config.json");

        await JsonConfig.UpdateAsync(
            configPath,
            async config =>
            {
                await Task.Yield();
                JsonConfig.Set(config, "Async:Value", true);
            });

        var root = await ReadJsonAsync(configPath);
        Assert.True(root["Async"]?["Value"]?.GetValue<bool>());
    }

    private static async Task<JsonNode> ReadJsonAsync(string path)
    {
        var json = await File.ReadAllTextAsync(path);

        return JsonNode.Parse(json) ?? throw new InvalidOperationException("JSON was empty.");
    }
}
