using Infrastructure.States;

namespace Infrastructure.Services;

public interface ICoreStateMachine
{
    CoreState GetCoreState();
    bool HasActiveCoreOperation();
    void DispatchCoreState();
    void DispatchCoreOperationState(CoreOperationState state);
    void DispatchInstallState(string jobId, InstallCoreState state);
    InstallCoreState? GetInstallCoreState();
    InstallCoreState? GetInstallCoreState(string jobId);
}
