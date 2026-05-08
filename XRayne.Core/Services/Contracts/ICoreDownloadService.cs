using XRayne.Repositories.External;

namespace XRayne.Core.Services;

public interface ICoreDownloadService
{
    Task<ICollection<GitHubRelease>> GetReleasesAsync(GithubRepositoriesFilter filter, bool noCache = false, CancellationToken ct = default);
}