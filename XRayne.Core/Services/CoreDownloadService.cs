using Microsoft.Extensions.Caching.Memory;
using XRayne.Repositories.External;

namespace XRayne.Core.Services;

public sealed class CoreDownloadService(IMemoryCache cache) : ICoreDownloadService
{
    private readonly GitHubRepository xrayRepository = new GitHubRepository("https://github.com/xtls/xray-core");

    public async Task<ICollection<GitHubRelease>> GetReleasesAsync(GithubRepositoriesFilter filter, bool noCache = false, CancellationToken ct = default)
    {
        if (noCache)
        {
            return await xrayRepository.GetReleasesAsync(filter, ct);
        }

        var result = await cache.GetOrCreateAsync($"core_releases_{filter.PerPage}_{filter.Page}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            return xrayRepository.GetReleasesAsync(filter, ct);
        });

        return result!;
    }
}