using XRayne.Infrastructure.States;

namespace XRayne.Infrastructure.Services;

public interface ICoreStateMachine
{
    void DispatchInstallState(string jobId, InstallCoreState state);
    InstallCoreState? GetInstallCoreState();
    InstallCoreState? GetInstallCoreState(string jobId);
}
