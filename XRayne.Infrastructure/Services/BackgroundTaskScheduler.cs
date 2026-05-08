using Quartz;
using XRayne.Core.Tasks;

namespace XRayne.Infrastructure.Services;

public sealed class BackgroundTaskScheduler(ISchedulerFactory schedulerFactory) : IBackgroundTaskScheduler
{
    public async Task ScheduleInstallCore(string version, CancellationToken ct)
    {
        var scheduler = await schedulerFactory.GetScheduler(ct);
        var job = JobBuilder.Create<InstallCoreJob>()
           .WithIdentity(InstallCoreJob.JobKey)
           .UsingJobData(InstallCoreJob.VersionKey, version)
           .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(InstallCoreJob.TriggerKey)
            .ForJob(job)
            .StartNow()
            .Build();

        await scheduler.ScheduleJob(job, trigger, ct);
    }
}