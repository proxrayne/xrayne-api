using Quartz;
using XRayne.Infrastructure.States;
using XRayne.Infrastructure.Tasks;

namespace XRayne.Infrastructure.Services;

public sealed class BackgroundTaskScheduler(ISchedulerFactory schedulerFactory, ICoreStateMachine coreState) : IBackgroundTaskScheduler
{
    private readonly SemaphoreSlim scheduleLock = new(1, 1);

    public async Task<string> ScheduleInstallCore(string version, CancellationToken ct)
    {
        await scheduleLock.WaitAsync(ct);
        try
        {
            var scheduler = await schedulerFactory.GetScheduler(ct);
            if (await scheduler.CheckExists(InstallCoreJob.JobKey, ct))
            {
                throw new InvalidOperationException("Core installation is already scheduled or in progress.");
            }

            var jobId = Guid.NewGuid().ToString();
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

            coreState.DispatchInstallState(jobId, InstallCoreState.Queued(version));

            await scheduler.ScheduleJob(job, trigger, ct);

            return jobId;
        }
        finally
        {
            scheduleLock.Release();
        }
    }

    public async Task ScheduleCoreOperation(CoreOperation operation, CancellationToken ct)
    {
        await scheduleLock.WaitAsync(ct);
        try
        {
            if (coreState.HasActiveCoreOperation())
            {
                throw new InvalidOperationException("Core operation is already scheduled or in progress.");
            }

            var scheduler = await schedulerFactory.GetScheduler(ct);
            if (!await scheduler.CheckExists(CoreOperationJob.JobKey, ct))
            {
                var job = JobBuilder.Create<CoreOperationJob>()
                    .WithIdentity(CoreOperationJob.JobKey)
                    .StoreDurably()
                    .Build();

                await scheduler.AddJob(job, replace: false, ct);
            }

            var data = new JobDataMap
            {
                [CoreOperationJob.OperationKey] = operation.ToString(),
            };

            var trigger = TriggerBuilder.Create()
                .WithIdentity(new TriggerKey(CoreOperationJob.TriggerKeyPrefix, "core"))
                .ForJob(CoreOperationJob.JobKey)
                .UsingJobData(data)
                .StartNow()
                .Build();

            coreState.DispatchCoreOperationState(CoreOperationState.Queued(operation));

            await scheduler.ScheduleJob(trigger, ct);
        }
        finally
        {
            scheduleLock.Release();
        }
    }
}
