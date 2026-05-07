using XRayne.Cli.Models;

namespace XRayne.Cli.Services.Contracts;

public interface IGitHubReleaseService
{
    string Repository { get; }

    Task<GitHubRelease> ResolveReleaseAsync(
        string version,
        CancellationToken cancellationToken);

    Task DownloadAssetAsync(
        string assetUrl,
        string destinationPath,
        CancellationToken cancellationToken);
}
