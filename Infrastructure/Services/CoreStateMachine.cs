using Microsoft.Extensions.Caching.Memory;
using Infrastructure.States;
using Contracts.Enums;

namespace Infrastructure.Services;

public class CoreStateMachine(
    IMemoryCache cache,
    IEventStreamManager eventStreams,
    ICoreService coreService) : ICoreStateMachine
{
    private static readonly TimeSpan InstallCacheTimeout = TimeSpan.FromHours(6);
    private readonly Lock operationLock = new();
    private CoreOperationState? operationState;

    public const string CoreStateStreamKey = "core-state";
    public static string GetInstallCoreStreamKey(string jobId) => $"core-install:{jobId}";

    public CoreState GetCoreState()
    {
        var activeOperation = GetActiveCoreOperation();
        var installStates = cache.Get<Dictionary<string, InstallCoreState>>(nameof(InstallCoreState));
        var isInstalled = coreService.GetIsInstalled();
        CoreStatus? status = null;
        if (coreService.GetIsRunning())
        {
            status = CoreStatus.Started;
        }
        else if (activeOperation != null)
        {
            status = activeOperation.Operation switch
            {
                CoreOperation.Restart => CoreStatus.Restarting,
                CoreOperation.Start => CoreStatus.Starting,
                CoreOperation.Stop => CoreStatus.Stopping,
                _ => null
            };
        }
        else if (isInstalled)
        {
            status = CoreStatus.Stopped;
        }

        return new CoreState(
            isInstalled,
            status,
            installStates?.Values.Any(IsActiveInstallState) == true,
            coreService.TryGetVersion());
    }

    public bool HasActiveCoreOperation()
    {
        return GetActiveCoreOperation() is not null;
    }

    public void DispatchCoreState()
    {
        eventStreams.Dispatch(CoreStateStreamKey, GetCoreState());
    }

    public void DispatchCoreOperationState(CoreOperationState state)
    {
        lock (operationLock)
        {
            operationState = state;
        }

        DispatchCoreState();
    }

    public void DispatchInstallState(string jobId, InstallCoreState state)
    {
        var values = cache.Get<Dictionary<string, InstallCoreState>>(nameof(InstallCoreState)) ?? new();

        values[jobId] = state;

        cache.Set(nameof(InstallCoreState), values, InstallCacheTimeout);
        eventStreams.Dispatch(GetInstallCoreStreamKey(jobId), state);
        DispatchCoreState();
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

    private static bool IsActiveInstallState(InstallCoreState state)
    {
        return state.Step is InstallCoreStep.Queued
            or InstallCoreStep.Validation
            or InstallCoreStep.Downloading
            or InstallCoreStep.Extracting
            or InstallCoreStep.Installing;
    }

    private CoreOperationState? GetActiveCoreOperation()
    {
        lock (operationLock)
        {
            return operationState?.IsActive == true ? operationState : null;
        }
    }
}
