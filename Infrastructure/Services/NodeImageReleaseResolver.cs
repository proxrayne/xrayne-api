using Octokit;
using System.Reflection;

namespace Infrastructure.Services;

/// <summary>
/// Resolves Node Docker image archives from the public GitHub release assets.
/// </summary>
public sealed class NodeImageReleaseResolver : INodeImageReleaseResolver
{
    private const string LatestVersion = "latest";
    private const string RepositoryUrl = "https://github.com/proxrayne/xrayne-node";
    private const string AssetPrefix = "xrayne-node-image";

    public async Task<NodeImageReleaseAsset> ResolveAsync(CancellationToken cancellationToken)
    {
        using var repository = new GitHubReleaseClient(RepositoryUrl);

        var release = await ResolveLatestSupportedReleaseAsync(repository, cancellationToken);
        var imageTag = SanitizeDockerTag(release.TagName);
        var assetName = $"{AssetPrefix}-{imageTag}.tar.gz";
        var asset = release.Assets.SingleOrDefault(item =>
            string.Equals(item.Name, assetName, StringComparison.Ordinal));

        if (asset is null)
        {
            throw new InvalidOperationException(
                $"Release asset '{assetName}' was not found in release '{release.TagName}'.");
        }

        return new NodeImageReleaseAsset(
            release.TagName,
            imageTag,
            assetName,
            asset.BrowserDownloadUrl);
    }

    private static async Task<Release> ResolveLatestSupportedReleaseAsync(
        GitHubReleaseClient repository,
        CancellationToken cancellationToken)
    {
        var panelVersion = GetPanelVersion();
        if (panelVersion is null)
        {
            return await repository.GetReleaseAsync(LatestVersion, cancellationToken);
        }

        var latest = await repository.GetReleaseAsync(LatestVersion, cancellationToken);
        if (TryParseVersion(latest.TagName, out var latestVersion)
            && latestVersion <= panelVersion)
        {
            return latest;
        }

        var releases = await repository.GetReleasesAsync(100, 1, cancellationToken);
        var supported = releases
            .Where(release => !release.Draft && !release.Prerelease)
            .Select(release => new
            {
                Release = release,
                Parsed = TryParseVersion(release.TagName, out var parsed) ? parsed : null,
            })
            .Where(item => item.Parsed is not null && item.Parsed <= panelVersion)
            .OrderByDescending(item => item.Parsed)
            .FirstOrDefault();

        return supported?.Release
            ?? throw new InvalidOperationException(
                $"No Node release compatible with panel version '{panelVersion}' was found.");
    }

    private static Version? GetPanelVersion()
    {
        var assembly = Assembly.GetEntryAssembly();
        var value = assembly
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
            .Split('+', 2)[0]
            ?? assembly?.GetName().Version?.ToString();

        return TryParseVersion(value, out var version) ? version : null;
    }

    private static bool TryParseVersion(string? value, out Version? version)
    {
        version = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim().TrimStart('v', 'V');
        var suffixIndex = normalized.IndexOfAny(['-', '+']);
        if (suffixIndex >= 0)
        {
            normalized = normalized[..suffixIndex];
        }

        if (!Version.TryParse(normalized, out var parsed))
        {
            return false;
        }

        version = parsed;
        return true;
    }

    private static string SanitizeDockerTag(string value)
    {
        var chars = value.Select(character =>
            char.IsAsciiLetterOrDigit(character) || character is '_' or '.' or '-'
                ? character
                : '-').ToArray();
        var tag = new string(chars).Trim('-');

        return string.IsNullOrWhiteSpace(tag) ? "latest" : tag;
    }
}
