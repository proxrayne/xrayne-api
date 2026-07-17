using Contracts.Enums;
using Data.Contracts;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Tasks;

/// <summary>
/// Upload geo resource file to node.
/// </summary>
[DisallowConcurrentExecution]
public sealed class GeoResourceUploadJob(
    IGeoResourceRepository repository,
    ITempFileStorage fileStorage,
    INodeGeoResourceService service,
    INodeCoreService nodeCore,
    ILogger<GeoResourceUploadJob> logger) : IJob
{
    public const string GeoResourceIdKey = "geoResourceId";
    public const string TempFilepath = "tempFilepath";

    private const string GroupName = "geo-resources";

    /// <summary>
    /// Gets a job key for a single geo resource operation.
    /// </summary>
    public static JobKey GetJobKey(long identity) => new($"upload-resource-{identity}", GroupName);

    /// <summary>
    /// Gets a trigger key for a single geo resource operation.
    /// </summary>
    public static TriggerKey GetTriggerKey(long identity) => new($"upload-resource-{identity}", GroupName);

    public async Task Execute(IJobExecutionContext context)
    {
        var data = context.MergedJobDataMap;
        var ct = context.CancellationToken;

        var geoResourceId = data.GetLong(GeoResourceIdKey);
        var tempFilepath = data.GetString(TempFilepath) ?? throw new FileNotFoundException($"Filepath is required.");

        var resource = await repository.GetByIdAsync(geoResourceId, ct);

        try
        {
            await service.UpdateStatusAsync(resource, GeoResourceStatus.Transferring, "Transferring geo resource to the remote node.", ct);

            await using var content = File.OpenRead(tempFilepath);

            await service.UploadToNodeAsync(resource, content, ct);

            await service.UpdateStatusAsync(resource, GeoResourceStatus.Success, ct);

            fileStorage.Delete(tempFilepath, ct);

            await nodeCore.RestartCoreAsync(resource.Node, ct);
        }
        catch (TaskCanceledException ex)
        {
            await service.UpdateStatusAsync(resource, GeoResourceStatus.Error, "Operation canceled.", ct);
            logger.LogError(ex, "Upload geo resource operation canceled.");
        }
        catch (Exception ex)
        {
            await service.UpdateStatusAsync(resource, GeoResourceStatus.Error, ex.Message, ct);
            logger.LogError(ex, "Upload geo resource error.");
        }
    }
}