using Microsoft.Extensions.Logging;
using Quartz;
using Infrastructure.Services;

namespace Infrastructure.Tasks;

/// <summary>
/// Refreshes due auto-updated geo resources.
/// </summary>
[DisallowConcurrentExecution]
public sealed class GeoResourceAutoUpdateJob(
    INodeGeoResourceService geoResources,
    ILogger<GeoResourceAutoUpdateJob> logger) : IJob
{
    /// <summary>
    /// Gets the recurring auto-update job key.
    /// </summary>
    public static readonly JobKey JobKey = new(nameof(GeoResourceAutoUpdateJob), "geo-resources");

    /// <summary>
    /// Gets the recurring auto-update trigger key.
    /// </summary>
    public static readonly TriggerKey TriggerKey = new(nameof(GeoResourceAutoUpdateJob), "geo-resources");

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await geoResources.RefreshDueAutoUpdatesAsync(context.CancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Failed to refresh due geo resources.");
        }
    }
}
