using Microsoft.Extensions.Caching.Memory;
using XRayne.Infrastructure.States;

namespace XRayne.Infrastructure.Services;

public class CoreStateMachine(IMemoryCache cache, IEventStreamManager eventStreams) : ICoreStateMachine
{
    private static readonly TimeSpan InstallCacheTimeout = TimeSpan.FromHours(6);

    public static string GetInstallCoreStreamKey(string jobId) => $"core-install:{jobId}";

    public void DispatchInstallState(string jobId, InstallCoreState state)
    {
        var values = cache.Get<Dictionary<string, InstallCoreState>>(nameof(InstallCoreState)) ?? new();

        values[jobId] = state;

        cache.Set(nameof(InstallCoreState), values, InstallCacheTimeout);
        eventStreams.Dispatch(GetInstallCoreStreamKey(jobId), state);
    }

    public InstallCoreState? GetInstallCoreState() => cache.Get<Dictionary<string, InstallCoreState>>(nameof(InstallCoreState))?.FirstOrDefault().Value;

    public InstallCoreState? GetInstallCoreState(string jobId)
    {
        if (cache.TryGetValue<Dictionary<string, InstallCoreState>>(nameof(InstallCoreState), out var values))
        {
            return values?[jobId];
        }

        return null;
    }
}
