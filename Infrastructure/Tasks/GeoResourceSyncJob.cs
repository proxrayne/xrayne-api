using Microsoft.Extensions.Logging;
using Quartz;
using Infrastructure.Services;

namespace Infrastructure.Tasks;

/// <summary>
/// Synchronizes geo resource metadata for enabled remote nodes.
/// </summary>
[DisallowConcurrentExecution]
public sealed class GeoResourceSyncJob(
    INodeService nodes,
    INodeGeoResourceService geoResources,
    ILogger<GeoResourceSyncJob> logger) : IJob
{
    /// <summary>
    /// Gets the recurring sync job key.
    /// </summary>
    public static readonly JobKey JobKey = new(nameof(GeoResourceSyncJob), "geo-resources");

    /// <summary>
    /// Gets the recurring sync trigger key.
    /// </summary>
    public static readonly TriggerKey TriggerKey = new(nameof(GeoResourceSyncJob), "geo-resources");

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        var allNodes = await nodes.GetAllAsync(context.CancellationToken);
        foreach (var node in allNodes.Where(node => node.Enabled))
        {
            try
            {
                await geoResources.SynchronizeNodeAsync(
                    node.Admin.Id,
                    node,
                    context.CancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Failed to synchronize geo resources for node {NodeId}.", node.Id);
            }
        }
    }
}
