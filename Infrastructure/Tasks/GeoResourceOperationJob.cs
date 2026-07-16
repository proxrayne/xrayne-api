using Infrastructure.Services;
using Infrastructure.States;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Infrastructure.Tasks;

/// <summary>
/// Executes one queued geo resource upload or refresh operation.
/// </summary>
[DisallowConcurrentExecution]
public sealed class GeoResourceOperationJob(
    INodeGeoResourceService geoResources,
    ILogger<GeoResourceOperationJob> logger) : IJob
{
    /// <summary>
    /// Gets the geo resource id job data key.
    /// </summary>
    public const string GeoResourceIdKey = "geoResourceId";

    /// <summary>
    /// Gets the operation job data key.
    /// </summary>
    public const string OperationKey = "operation";

    /// <summary>
    /// Gets the optional upload file path job data key.
    /// </summary>
    public const string UploadFilePathKey = "uploadFilePath";

    /// <summary>
    /// Gets the optional previous file name job data key.
    /// </summary>
    public const string PreviousFileNameKey = "previousFileName";

    /// <summary>
    /// Gets a job key for a single geo resource operation.
    /// </summary>
    public static JobKey GetJobKey(string identity) => new($"geo-resource-{identity}", "geo-resources");

    /// <summary>
    /// Gets a trigger key for a single geo resource operation.
    /// </summary>
    public static TriggerKey GetTriggerKey(string identity) => new($"geo-resource-{identity}", "geo-resources");

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        var data = context.MergedJobDataMap;
        var geoResourceId = data.GetLong(GeoResourceIdKey);
        var operationValue = data.GetString(OperationKey);
        var uploadFilePath = GetOptionalString(data, UploadFilePathKey);
        var previousFileName = GetOptionalString(data, PreviousFileNameKey);

        if (!Enum.TryParse<GeoResourceOperation>(operationValue, out var operation))
        {
            logger.LogWarning(
                "Geo resource operation {Operation} for resource {GeoResourceId} is invalid.",
                operationValue,
                geoResourceId);
            return;
        }

        await geoResources.ExecuteQueuedOperationAsync(
            geoResourceId,
            operation,
            uploadFilePath,
            previousFileName,
            context.CancellationToken);
    }

    private static string? GetOptionalString(JobDataMap data, string key)
    {
        return data.ContainsKey(key) ? data.GetString(key) : null;
    }
}
