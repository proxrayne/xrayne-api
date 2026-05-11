namespace XRayne.Infrastructure.Services;

public interface IBackgroundTaskScheduler
{
    Task<string> ScheduleInstallCore(string version, CancellationToken ct);
}