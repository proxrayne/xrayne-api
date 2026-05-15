using Quartz;
using XRayne.Infrastructure.States;
using XRayne.Infrastructure.Tasks;

namespace XRayne.Infrastructure.Services;

public sealed class BackgroundTaskScheduler(ISchedulerFactory schedulerFactory, ICoreStateMachine coreState) : IBackgroundTaskScheduler
{
    public async Task<string> ScheduleInstallCore(string version, CancellationToken ct)
    {
        var scheduler = await schedulerFactory.GetScheduler(ct);
        if (await scheduler.CheckExists(InstallCoreJob.JobKey, ct))
        {
            throw new InvalidOperationException("Core installation is already scheduled or in progress.");
        }

        var jobId = Guid.NewGuid().ToString("D");
        var job = JobBuilder.Create<InstallCoreJob>()
           .WithIdentity(InstallCoreJob.JobKey)
           .UsingJobData(InstallCoreJob.VersionKey, version)
           .UsingJobData(InstallCoreJob.IdentityKey, jobId)
           .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(InstallCoreJob.TriggerKey)
            .ForJob(job)
            .StartNow()
            .Build();


        await scheduler.ScheduleJob(job, trigger, ct);

        coreState.DispatchInstallState(jobId, InstallCoreState.Queued(version));

        return jobId;
    }
}
