using System.Net.Http.Headers;
using System.Text.Json;
using XRayne.Cli.Models;
using XRayne.Cli.Services.Contracts;

namespace XRayne.Cli.Services;

public sealed class GitHubReleaseService : IGitHubReleaseService
{
    private const string LatestVersion = "latest";

    public string Repository => "VanyaKrotov/xrayne";

    public async Task<GitHubRelease> ResolveReleaseAsync(
        string version,
        CancellationToken cancellationToken)
    {
        var url = string.Equals(version, LatestVersion, StringComparison.OrdinalIgnoreCase)
            ? $"https://api.github.com/repos/{Repository}/releases/latest"
            : $"https://api.github.com/repos/{Repository}/releases/tags/{Uri.EscapeDataString(version)}";

        using var client = CreateGitHubClient();
        using var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        if (root.TryGetProperty("prerelease", out var prereleaseElement) && prereleaseElement.GetBoolean())
        {
            throw new InvalidOperationException("Pre-release versions are not supported. Use a stable release tag.");
        }

        var tagName = root.GetProperty("tag_name").GetString()
            ?? throw new InvalidOperationException("GitHub release response does not contain tag_name.");
        var assets = root.GetProperty("assets")
            .EnumerateArray()
            .Select(item => new GitHubAsset(
                item.GetProperty("name").GetString() ?? string.Empty,
                item.GetProperty("url").GetString() ?? string.Empty))
            .Where(item => !string.IsNullOrWhiteSpace(item.Name) && !string.IsNullOrWhiteSpace(item.DownloadUrl))
            .ToArray();

        return new GitHubRelease(tagName, assets);
    }

    public async Task DownloadAssetAsync(
        string assetUrl,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        using var client = CreateGitHubClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

        using var response = await client.GetAsync(assetUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var output = File.Create(destinationPath);
        await input.CopyToAsync(output, cancellationToken);
    }

    private static HttpClient CreateGitHubClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("xrayne-cli", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        return client;
    }
}
