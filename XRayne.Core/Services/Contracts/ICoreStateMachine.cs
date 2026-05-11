using XRayne.Core.States;

namespace XRayne.Core.Services;

public interface ICoreStateMachine
{
    void DispatchInstallState(string jobId, InstallCoreState state);
    InstallCoreState? GetInstallCoreState();
    InstallCoreState? GetInstallCoreState(string jobId);
}