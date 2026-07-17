using Data.Contracts;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Tasks;

/// <summary>
/// Refreshes due auto-updated geo resources.
/// </summary>
[DisallowConcurrentExecution]
public sealed class UpdateGeoResourcesJob(
    IGeoResourceRepository geoResources,
    INodeGeoResourceService service,
    ILogger<UpdateGeoResourcesJob> logger) : IJob
{
    private const string GroupName = "geo-resources";

    /// <summary>
    /// Gets the recurring auto-update job key.
    /// </summary>
    public static readonly JobKey JobKey = new(nameof(UpdateGeoResourcesJob), GroupName);

    /// <summary>
    /// Gets the recurring auto-update trigger key.
    /// </summary>
    public static readonly TriggerKey TriggerKey = new(nameof(UpdateGeoResourcesJob), GroupName);

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var dueGeoResources = await geoResources.GetDueAutoUpdateIdsAsync(DateTimeOffset.UtcNow, context.CancellationToken);
            foreach (var entity in dueGeoResources)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                await service.ScheduleDownloadAutoUpdatesAsync(entity, context.CancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("Update resources is canceled.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Failed to refresh due geo resources.");
        }
    }
}
