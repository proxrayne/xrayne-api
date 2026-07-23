using Infrastructure.Tasks;
using Quartz;

namespace Infrastructure.Services;

public sealed class BackgroundTaskScheduler(ISchedulerFactory schedulerFactory) : IBackgroundTaskScheduler
{
    public async Task ScheduleGeoResourceDownload(long geoResourceId, CancellationToken ct)
    {
        var scheduler = await schedulerFactory.GetScheduler(ct);
        var jobKey = GeoResourceDownloadJob.GetJobKey(geoResourceId);
        if (await scheduler.CheckExists(jobKey, ct))
        {
            throw new InvalidOperationException("Update current resource is already scheduled or in progress.");
        }

        var data = new JobDataMap
        {
            [GeoResourceDownloadJob.GeoResourceIdKey] = geoResourceId
        };

        var job = JobBuilder.Create<GeoResourceDownloadJob>()
            .WithIdentity(jobKey)
            .UsingJobData(data)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(GeoResourceDownloadJob.GetTriggerKey(geoResourceId))
            .ForJob(job)
            .StartNow()
            .Build();

        await scheduler.ScheduleJob(job, trigger, ct);
    }

    public async Task ScheduleGeoResourceUpload(long geoResourceId, string filepath, CancellationToken ct)
    {
        var scheduler = await schedulerFactory.GetScheduler(ct);
        var jobKey = GeoResourceUploadJob.GetJobKey(geoResourceId);
        if (await scheduler.CheckExists(jobKey, ct))
        {
            throw new InvalidOperationException("Upload current resource is already scheduled or in progress.");
        }

        var data = new JobDataMap
        {
            [GeoResourceUploadJob.GeoResourceIdKey] = geoResourceId,
            [GeoResourceUploadJob.TempFilepath] = filepath
        };

        var job = JobBuilder.Create<GeoResourceUploadJob>()
            .WithIdentity(jobKey)
            .UsingJobData(data)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(GeoResourceUploadJob.GetTriggerKey(geoResourceId))
            .ForJob(job)
            .StartNow()
            .Build();

        await scheduler.ScheduleJob(job, trigger, ct);
    }
}
