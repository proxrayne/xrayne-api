using Contracts.Enums;
using Data.Contracts;
using Infrastructure.Dto;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Tasks;

/// <summary>
/// Download and upload to node auto-update geo resources. 
/// </summary>
[DisallowConcurrentExecution]
public sealed class GeoResourceDownloadJob(
    IGeoResourceRepository repository,
    INodeGeoResourceService service,
    INodeCoreService nodeCore,
    ILogger<GeoResourceDownloadJob> logger) : IJob
{
    public const string GeoResourceIdKey = "geoResourceId";
    private const string GroupName = "geo-resources";

    /// <summary>
    /// Gets a job key for a single geo resource operation.
    /// </summary>
    public static JobKey GetJobKey(long identity) => new($"download-resource-{identity}", GroupName);

    /// <summary>
    /// Gets a trigger key for a single geo resource operation.
    /// </summary>
    public static TriggerKey GetTriggerKey(long identity) => new($"download-resource-{identity}", GroupName);

    public async Task Execute(IJobExecutionContext context)
    {
        var data = context.MergedJobDataMap;
        var ct = context.CancellationToken;

        var geoResourceId = data.GetLong(GeoResourceIdKey);

        var resource = await repository.GetByIdAsync(geoResourceId, ct);

        try
        {
            if (string.IsNullOrWhiteSpace(resource.Url) || resource.UpdateInterval == null)
            {
                throw new NodeGeoResourceValidationException("Auto-updated geo resource is missing URL or cron template.");
            }

            await service.UpdateStatusAsync(resource, GeoResourceStatus.Loading, "Downloading geo resource content from URL.", ct);

            await using var content = await service.DownloadAsync(resource.Url, ct);

            await service.UpdateStatusAsync(resource, GeoResourceStatus.Transferring, "Transferring geo resource to the remote node.", ct);

            await service.UploadToNodeAsync(resource, content, ct);

            await service.UpdateStatusAsync(resource, GeoResourceStatus.Success, ct);

            await nodeCore.RestartCoreAsync(resource.Node, ct);
        }
        catch (TaskCanceledException ex)
        {
            await service.UpdateStatusAsync(resource, GeoResourceStatus.Error, $"Download operation canceled.", ct);

            logger.LogError(ex, "Failure download & upload geo resource.");
        }
        catch (Exception ex)
        {
            await service.UpdateStatusAsync(resource, GeoResourceStatus.Error, $"Error: {ex.Message}", ct);

            logger.LogError(ex, "Failure download & upload geo resource.");
        }
    }
}