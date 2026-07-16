using Infrastructure.States;

namespace Infrastructure.Services;

/// <summary>
/// Schedules background operations through the application scheduler.
/// </summary>
public interface IBackgroundTaskScheduler
{
    /// <summary>
    /// Schedules xray-core installation.
    /// </summary>
    Task<string> ScheduleInstallCore(string version, CancellationToken ct);

    /// <summary>
    /// Schedules remote node provisioning.
    /// </summary>
    Task<string> ScheduleProvisionNode(long nodeId, CancellationToken ct);

    /// <summary>
    /// Schedules an xray-core runtime operation.
    /// </summary>
    Task ScheduleCoreOperation(CoreOperation operation, CancellationToken ct);

    /// <summary>
    /// Schedules a geo resource operation.
    /// </summary>
    Task ScheduleGeoResourceOperation(
        long geoResourceId,
        GeoResourceOperation operation,
        string? uploadFilePath,
        string? previousFileName,
        CancellationToken ct);
}
