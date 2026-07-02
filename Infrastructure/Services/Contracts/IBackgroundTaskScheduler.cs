using XRayne.Infrastructure.States;

namespace XRayne.Infrastructure.Services;

public interface IBackgroundTaskScheduler
{
    Task<string> ScheduleInstallCore(string version, CancellationToken ct);
    Task<string> ScheduleProvisionNode(long nodeId, CancellationToken ct);
    Task ScheduleCoreOperation(CoreOperation operation, CancellationToken ct);
}
