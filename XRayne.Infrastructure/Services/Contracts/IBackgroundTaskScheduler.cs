namespace XRayne.Infrastructure.Services;

public interface IBackgroundTaskScheduler
{
    Task ScheduleInstallCore(string version, CancellationToken ct);
}