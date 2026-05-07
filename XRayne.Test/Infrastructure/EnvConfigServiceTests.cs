using XRayne.Infrastructure.Services;

namespace XRayne.Test.Infrastructure;

public sealed class EnvConfigServiceTests
{
    [Fact]
    public async Task SaveAsync_WritesEscapedValuesAndFromPathReadsThemBack()
    {
        using var workspace = new TestWorkspace();
        var envPath = workspace.GetPath("runtime", ".env");
        var service = new EnvConfigService(envPath);

        service.Set("SIMPLE", "value");
        service.Set("SECRET", "pa ss#=$\"'word");

        await service.SaveAsync();

        var lines = await File.ReadAllLinesAsync(envPath);
        Assert.Contains("SIMPLE=value", lines);
        Assert.Contains("SECRET='pa ss#=$\"'\\''word'", lines);

        var loaded = await EnvConfigService.FromPath(envPath);
        Assert.Equal("value", loaded.Get("SIMPLE"));
        Assert.Equal("pa ss#=$\"'word", loaded.Get("SECRET"));
    }

    [Fact]
    public async Task Set_UpdatesExistingKeyCaseInsensitivelyAndPreservesComments()
    {
        using var workspace = new TestWorkspace();
        var envPath = workspace.GetPath(".env");
        await File.WriteAllLinesAsync(
            envPath,
            [
                "# keep me",
                "API_IMAGE=old",
                "OTHER=value"
            ]);

        var service = await EnvConfigService.FromPath(envPath);
        service.Set("api_image", "new");

        await service.SaveAsync();

        var lines = await File.ReadAllLinesAsync(envPath);
        Assert.Equal("# keep me", lines[0]);
        Assert.Contains("api_image=new", lines);
        Assert.Contains("OTHER=value", lines);
        Assert.DoesNotContain("API_IMAGE=old", lines);
    }

    [Fact]
    public async Task Remove_DeletesAllMatchingKeys()
    {
        using var workspace = new TestWorkspace();
        var envPath = workspace.GetPath(".env");
        await File.WriteAllLinesAsync(
            envPath,
            [
                "POSTGRES_HOST_API=postgres",
                "OTHER=value",
                "postgres_host_api=localhost"
            ]);

        var service = await EnvConfigService.FromPath(envPath);
        service.Remove("POSTGRES_HOST_API");

        await service.SaveAsync();

        var lines = await File.ReadAllLinesAsync(envPath);
        Assert.Equal(["OTHER=value"], lines);
    }

    [Fact]
    public async Task SetAndSaveAsync_CreatesMissingFileAndDirectory()
    {
        using var workspace = new TestWorkspace();
        var envPath = workspace.GetPath("new", ".env");

        await EnvConfigService.SetAndSaveAsync("API_PORT", "5000", envPath);

        var loaded = await EnvConfigService.FromPath(envPath);
        Assert.Equal("5000", loaded.Get("API_PORT"));
    }
}
