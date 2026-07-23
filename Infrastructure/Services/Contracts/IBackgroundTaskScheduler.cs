namespace Infrastructure.Services;

/// <summary>
/// Schedules background operations through the application scheduler.
/// </summary>
public interface IBackgroundTaskScheduler
{
    /// <summary>
    /// Schedules a geo resource download & upload file.
    /// </summary>
    Task ScheduleGeoResourceDownload(long geoResourceId, CancellationToken ct);

    /// <summary>
    /// Schedules a geo resource download & upload file.
    /// </summary>
    Task ScheduleGeoResourceUpload(long geoResourceId, string filepath, CancellationToken ct);
}
