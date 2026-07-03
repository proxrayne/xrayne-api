using Infrastructure.States;

namespace Infrastructure.Services;

public interface IBackgroundTaskScheduler
{
    Task<string> ScheduleInstallCore(string version, CancellationToken ct);
    Task<string> ScheduleProvisionNode(long nodeId, CancellationToken ct);
    Task ScheduleCoreOperation(CoreOperation operation, CancellationToken ct);
}
