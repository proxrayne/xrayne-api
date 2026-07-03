using Repositories.Utilities;

namespace Test.Infrastructure;

public sealed class EnvConfigTests
{
    [Fact]
    public async Task SetAsync_WritesEscapedValuesAndGetAsyncReadsThemBack()
    {
        using var workspace = new TestWorkspace();
        var envPath = workspace.GetPath("runtime", ".env");

        await EnvConfig.SetAsync("SIMPLE", "value", envPath);
        await EnvConfig.SetAsync("SECRET", "pa ss#=$\"'word", envPath);

        var lines = await File.ReadAllLinesAsync(envPath);
        Assert.Contains("SIMPLE=value", lines);
        Assert.Contains("SECRET='pa ss#=$\"'\\''word'", lines);
        Assert.Equal("value", await EnvConfig.GetAsync("SIMPLE", envPath));
        Assert.Equal("pa ss#=$\"'word", await EnvConfig.GetAsync("SECRET", envPath));
    }

    [Fact]
    public async Task SetAsync_UpdatesExistingKeyCaseInsensitivelyAndPreservesComments()
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

        await EnvConfig.SetAsync("api_image", "new", envPath);

        var lines = await File.ReadAllLinesAsync(envPath);
        Assert.Equal("# keep me", lines[0]);
        Assert.Contains("api_image=new", lines);
        Assert.Contains("OTHER=value", lines);
        Assert.DoesNotContain("API_IMAGE=old", lines);
    }

    [Fact]
    public async Task RemoveAsync_DeletesAllMatchingKeys()
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

        await EnvConfig.RemoveAsync("POSTGRES_HOST_API", envPath);

        var lines = await File.ReadAllLinesAsync(envPath);
        Assert.Equal(["OTHER=value"], lines);
    }

    [Fact]
    public async Task RemoveAsync_CanRunCallbackAfterRemoval()
    {
        using var workspace = new TestWorkspace();
        var envPath = workspace.GetPath(".env");
        await File.WriteAllLinesAsync(
            envPath,
            [
                "API_IMAGE=old",
                "OTHER=value"
            ]);

        await EnvConfig.RemoveAsync(
            "API_IMAGE",
            envPath,
            env => EnvConfig.Set(env, "API_PORT", "5000"));

        var lines = await File.ReadAllLinesAsync(envPath);
        Assert.DoesNotContain("API_IMAGE=old", lines);
        Assert.Contains("OTHER=value", lines);
        Assert.Contains("API_PORT=5000", lines);
    }

    [Fact]
    public async Task RemoveAsync_CanRunAsyncCallbackAfterRemoval()
    {
        using var workspace = new TestWorkspace();
        var envPath = workspace.GetPath(".env");
        await File.WriteAllTextAsync(envPath, "API_IMAGE=old");

        await EnvConfig.RemoveAsync(
            "API_IMAGE",
            envPath,
            async env =>
            {
                await Task.Yield();
                EnvConfig.Set(env, "ASYNC", "true");
            });

        var lines = await File.ReadAllLinesAsync(envPath);
        Assert.DoesNotContain("API_IMAGE=old", lines);
        Assert.Contains("ASYNC=true", lines);
    }

    [Fact]
    public async Task SetAsync_CreatesMissingFileAndDirectory()
    {
        using var workspace = new TestWorkspace();
        var envPath = workspace.GetPath("new", ".env");

        await EnvConfig.SetAsync("API_PORT", "5000", envPath);

        Assert.Equal("5000", await EnvConfig.GetAsync("API_PORT", envPath));
    }

    [Fact]
    public async Task UpdateValueAsync_UsesCurrentValue()
    {
        using var workspace = new TestWorkspace();
        var envPath = workspace.GetPath(".env");
        await File.WriteAllTextAsync(envPath, "API_PORT=5000");

        await EnvConfig.UpdateValueAsync(
            "API_PORT",
            current => current == "5000" ? "5001" : "5000",
            envPath);

        Assert.Equal("5001", await EnvConfig.GetAsync("API_PORT", envPath));
    }

    [Fact]
    public async Task UpdateAsync_PreservesConcurrentUpdates()
    {
        using var workspace = new TestWorkspace();
        var envPath = workspace.GetPath(".env");

        await Task.WhenAll(
            EnvConfig.UpdateAsync(envPath, env => EnvConfig.Set(env, "FIRST", "one")),
            EnvConfig.UpdateAsync(envPath, env => EnvConfig.Set(env, "SECOND", "two")));

        Assert.Equal("one", await EnvConfig.GetAsync("FIRST", envPath));
        Assert.Equal("two", await EnvConfig.GetAsync("SECOND", envPath));
        Assert.Empty(Directory.GetFiles(workspace.Root, "*.tmp"));
    }

    [Fact]
    public async Task UpdateAsync_CanRunAsyncCallback()
    {
        using var workspace = new TestWorkspace();
        var envPath = workspace.GetPath(".env");

        await EnvConfig.UpdateAsync(
            envPath,
            async env =>
            {
                await Task.Yield();
                EnvConfig.Set(env, "ASYNC", "true");
            });

        Assert.Equal("true", await EnvConfig.GetAsync("ASYNC", envPath));
    }
}
