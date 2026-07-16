using System.Net;
using Contracts.Enums;
using Contracts.Utilities;
using Contracts.Values;
using Data.Contracts;
using Data.Entities;
using Infrastructure.Dto;
using Infrastructure.States;
using Microsoft.Extensions.Logging;
using Quartz;
using Node.Exceptions;
using Node.Models;
using Node.Services;

namespace Infrastructure.Services;

/// <summary>
/// Manages geo resource metadata and remote node file synchronization.
/// </summary>
public sealed class NodeGeoResourceService(
    IGeoResourceRepository geoResources,
    INodeSecretService secrets,
    INodeGeoResourceClientFactory geoResourceClientFactory,
    INodeCoreClientFactory coreClientFactory,
    INodeCoreStateStore coreStateStore,
    INodeCoreConfigBuilder coreConfigBuilder,
    IHttpClientFactory httpClientFactory,
    IBackgroundTaskScheduler scheduler,
    ILogger<NodeGeoResourceService> logger) : INodeGeoResourceService
{
    private const int MaxFilenameLength = 128;
    private const string GeoDownloadClientName = "geo-resources";

    /// <inheritdoc />
    public async Task<List<GeoResourceEntity>> SynchronizeNodeAsync(
        Guid adminId,
        NodeEntity node,
        CancellationToken cancellationToken = default)
    {
        var remote = await CreateClient(node).GetGeoResourcesAsync(cancellationToken);
        var existing = await geoResources.GetAllAsync(adminId, node.Id, cancellationToken);
        var remoteByName = remote.ToDictionary(
            resource => NormalizeFileName(resource.FileName),
            StringComparer.OrdinalIgnoreCase);

        foreach (var stale in existing
            .Where(resource =>
                resource.Status == GeoResourceStatus.Success &&
                !remoteByName.ContainsKey(resource.Filename))
            .ToList())
        {
            _ = await geoResources.DeleteAsync(adminId, stale.Id, cancellationToken);
        }

        foreach (var remoteResource in remote)
        {
            _ = await UpsertFromRemoteAsync(
                adminId,
                node,
                remoteResource,
                null,
                null,
                null,
                preserveExistingMetadata: true,
                cancellationToken: cancellationToken);
        }

        return await geoResources.GetAllAsync(adminId, node.Id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<GeoResourceEntity>> GetAllAsync(
        Guid adminId,
        NodeEntity node,
        CancellationToken cancellationToken = default)
    {
        return geoResources.GetAllAsync(adminId, node.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GeoResourceEntity>  CreateFileAsync(
        Guid adminId,
        NodeEntity node,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var normalizedFileName = NormalizeFileName(fileName);
        await EnsureUniqueAsync(adminId, node.Id, normalizedFileName, null, cancellationToken);

        var uploadFilePath = await SaveUploadAsync(content, cancellationToken);
        var created = await geoResources.AddAsync(new GeoResourceEntity
        {
            Filename = normalizedFileName,
            SizeBytes = 0,
            LastModifiedAt = DateTimeOffset.UtcNow,
            Status = GeoResourceStatus.Queued,
            StatusMessage = "Queued uploaded file transfer.",
            Node = node,
            Admin = node.Admin
        }, cancellationToken);

        await ScheduleOperationOrFailAsync(
            created,
            created.Id,
            GeoResourceOperation.UploadFile,
            uploadFilePath,
            null,
            cancellationToken);

        return created;
    }

    /// <inheritdoc />
    public async Task<GeoResourceEntity> CreateAutoUpdateAsync(
        Guid adminId,
        NodeEntity node,
        string fileName,
        string url,
        string cronTemplate,
        CancellationToken cancellationToken = default)
    {
        var normalizedFileName = NormalizeFileName(fileName);
        var normalizedUrl = NormalizeUrl(url);
        var normalizedCron = NormalizeCron(cronTemplate);
        await EnsureUniqueAsync(adminId, node.Id, normalizedFileName, null, cancellationToken);

        var created = await geoResources.AddAsync(new GeoResourceEntity
        {
            Filename = normalizedFileName,
            SizeBytes = 0,
            LastModifiedAt = DateTimeOffset.UtcNow,
            Status = GeoResourceStatus.Queued,
            StatusMessage = "Queued URL download.",
            Url = normalizedUrl,
            CronTemplate = normalizedCron,
            Node = node,
            Admin = node.Admin
        }, cancellationToken);

        await ScheduleOperationOrFailAsync(
            created,
            created.Id,
            GeoResourceOperation.Refresh,
            null,
            null,
            cancellationToken);

        return created;
    }

    /// <inheritdoc />
    public async Task<GeoResourceEntity> UpdateAsync(
        Guid adminId,
        NodeEntity node,
        long id,
        string fileName,
        string? url,
        string? cronTemplate,
        CancellationToken cancellationToken = default)
    {
        var resource = await GetResourceAsync(adminId, node.Id, id, cancellationToken);
        var nextFileName = NormalizeFileName(fileName);
        await EnsureUniqueAsync(adminId, node.Id, nextFileName, resource.Id, cancellationToken);

        var isAutoUpdate = resource.IsAutoUpdate;
        var nextUrl = isAutoUpdate ? NormalizeUrl(url) : null;
        var nextCron = isAutoUpdate ? NormalizeCron(cronTemplate) : null;
        var fileNameChanged = !string.Equals(resource.Filename, nextFileName, StringComparison.Ordinal);
        var refreshRequired = isAutoUpdate
            && (!string.Equals(resource.Url, nextUrl, StringComparison.Ordinal)
                || !string.Equals(resource.CronTemplate, nextCron, StringComparison.Ordinal));
        var previousFileName = fileNameChanged ? resource.Filename : null;

        resource.Filename = nextFileName;
        resource.Url = nextUrl;
        resource.CronTemplate = nextCron;
        resource.NextRunAt = isAutoUpdate ? resource.NextRunAt : null;
        resource.LastErrorAt = null;
        resource.Status = GeoResourceStatus.Queued;
        resource.StatusMessage = refreshRequired
            ? "Queued geo resource refresh."
            : "Queued geo resource metadata update.";

        var updated = await geoResources.UpdateAsync(adminId, resource, cancellationToken)
            ?? throw new NodeGeoResourceNotFoundException($"Geo resource '{id}' was not found.");

        await ScheduleOperationOrFailAsync(
            updated,
            updated.Id,
            GeoResourceOperation.Refresh,
            null,
            previousFileName,
            cancellationToken);

        return updated;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        Guid adminId,
        NodeEntity node,
        long id,
        CancellationToken cancellationToken = default)
    {
        var resource = await GetResourceAsync(adminId, node.Id, id, cancellationToken);
        try
        {
            await CreateClient(node).DeleteGeoResourceAsync(resource.Filename, cancellationToken);
        }
        catch (NodeHttpException exception) when (exception.StatusCode is HttpStatusCode.NotFound)
        {
        }

        _ = await geoResources.DeleteAsync(adminId, resource.Id, cancellationToken);
        await RestartCoreIfRunningAsync(node, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GeoResourceContent> DownloadAsync(
        Guid adminId,
        NodeEntity node,
        long id,
        CancellationToken cancellationToken = default)
    {
        var resource = await GetResourceAsync(adminId, node.Id, id, cancellationToken);
        EnsureAvailable(resource);

        return await CreateClient(node).DownloadGeoResourceAsync(resource.Filename, cancellationToken);
    }

    /// <inheritdoc />
    public async Task ScheduleDueAutoUpdatesAsync(CancellationToken cancellationToken = default)
    {
        var due = await geoResources.GetDueAutoUpdateAsync(DateTimeOffset.UtcNow, cancellationToken);
        foreach (var resource in due)
        {
            resource.Status = GeoResourceStatus.Queued;
            resource.StatusMessage = "Queued scheduled geo resource refresh.";
            _ = await geoResources.UpdateAsync(resource, cancellationToken);

            try
            {
                await scheduler.ScheduleGeoResourceOperation(
                    resource.Id,
                    GeoResourceOperation.Refresh,
                    null,
                    null,
                    cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                await FailOperationAsync(resource, exception, cancellationToken);
            }
        }
    }

    /// <inheritdoc />
    public async Task ExecuteQueuedOperationAsync(
        long id,
        GeoResourceOperation operation,
        string? uploadFilePath,
        string? previousFileName,
        CancellationToken cancellationToken = default)
    {
        var resource = await geoResources.GetByIdAsync(id, cancellationToken)
            ?? throw new NodeGeoResourceNotFoundException($"Geo resource '{id}' was not found.");

        try
        {
            switch (operation)
            {
                case GeoResourceOperation.UploadFile:
                    await ExecuteUploadFileAsync(resource, uploadFilePath, cancellationToken);
                    break;

                case GeoResourceOperation.Refresh:
                    await ExecuteRefreshAsync(resource, previousFileName, cancellationToken);
                    break;

                default:
                    throw new NodeGeoResourceValidationException("Geo resource operation is invalid.");
            }
        }
        finally
        {
            DeleteTemporaryUpload(uploadFilePath);
        }
    }

    private async Task ExecuteUploadFileAsync(
        GeoResourceEntity resource,
        string? uploadFilePath,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(uploadFilePath) || !File.Exists(uploadFilePath))
            {
                throw new NodeGeoResourceValidationException("Queued geo resource upload file is missing.");
            }

            await SetProgressAsync(
                resource,
                GeoResourceStatus.Loading,
                "Loaded uploaded file into the panel task.",
                cancellationToken);

            await using var content = File.OpenRead(uploadFilePath);
            await SetProgressAsync(
                resource,
                GeoResourceStatus.Transferring,
                "Transferring geo resource to the remote node.",
                cancellationToken);
            var remote = await CreateClient(resource.Node).UploadGeoResourceAsync(
                resource.Filename,
                content,
                cancellationToken);

            resource.SizeBytes = remote.SizeBytes;
            resource.LastModifiedAt = remote.LastModifiedAt;
            await CompleteOperationAsync(resource, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await FailOperationAsync(resource, exception, cancellationToken);
        }
    }

    private async Task ExecuteRefreshAsync(
        GeoResourceEntity resource,
        string? previousFileName,
        CancellationToken cancellationToken)
    {
        try
        {
            var previousName = string.IsNullOrWhiteSpace(previousFileName)
                ? null
                : NormalizeFileName(previousFileName);
            if (previousName is not null && !string.Equals(previousName, resource.Filename, StringComparison.Ordinal))
            {
                await SetProgressAsync(
                    resource,
                    GeoResourceStatus.Updating,
                    $"Renaming remote geo resource from '{previousName}' to '{resource.Filename}'.",
                    cancellationToken);
                var renamed = await CreateClient(resource.Node).RenameGeoResourceAsync(
                    previousName,
                    new RenameGeoResourceRequest(resource.Filename),
                    cancellationToken);
                resource.SizeBytes = renamed.SizeBytes;
                resource.LastModifiedAt = renamed.LastModifiedAt;
            }

            if (resource.IsAutoUpdate)
            {
                if (string.IsNullOrWhiteSpace(resource.Url) || string.IsNullOrWhiteSpace(resource.CronTemplate))
                {
                    throw new NodeGeoResourceValidationException("Auto-updated geo resource is missing URL or cron template.");
                }

                await SetProgressAsync(
                    resource,
                    GeoResourceStatus.Loading,
                    "Downloading geo resource content from URL.",
                    cancellationToken);
                await using var content = await DownloadAsync(resource.Url, cancellationToken);
                await SetProgressAsync(
                    resource,
                    GeoResourceStatus.Transferring,
                    "Transferring geo resource to the remote node.",
                    cancellationToken);
                var remote = await CreateClient(resource.Node).UploadGeoResourceAsync(
                    resource.Filename,
                    content,
                    cancellationToken);
                resource.SizeBytes = remote.SizeBytes;
                resource.LastModifiedAt = remote.LastModifiedAt;
                resource.NextRunAt = GetNextRun(resource.CronTemplate, DateTimeOffset.UtcNow);
            }

            await CompleteOperationAsync(resource, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await FailOperationAsync(resource, exception, cancellationToken);
        }
    }

    private async Task<GeoResourceEntity> UpsertFromRemoteAsync(
        Guid adminId,
        NodeEntity node,
        GeoResourceDto remoteResource,
        string? url,
        string? cronTemplate,
        DateTimeOffset? nextRunAt,
        bool preserveExistingMetadata,
        CancellationToken cancellationToken)
    {
        var fileName = NormalizeFileName(remoteResource.FileName);
        var resource = await geoResources.GetByFilenameAsync(adminId, node.Id, fileName, cancellationToken);
        if (resource is null)
        {
            resource = new GeoResourceEntity
            {
                Filename = fileName,
                SizeBytes = remoteResource.SizeBytes,
                LastModifiedAt = remoteResource.LastModifiedAt,
                Status = GeoResourceStatus.Success,
                Url = url,
                CronTemplate = cronTemplate,
                NextRunAt = nextRunAt,
                Node = node,
                Admin = node.Admin
            };

            return await geoResources.AddAsync(resource, cancellationToken);
        }

        resource.SizeBytes = remoteResource.SizeBytes;
        resource.LastModifiedAt = remoteResource.LastModifiedAt;
        if (!preserveExistingMetadata)
        {
            resource.Url = url;
            resource.CronTemplate = cronTemplate;
            resource.NextRunAt = nextRunAt;
            resource.LastErrorAt = null;
        }
        resource.Status = GeoResourceStatus.Success;
        resource.StatusMessage = null;
        resource.LastErrorAt = null;

        return await geoResources.UpdateAsync(adminId, resource, cancellationToken)
            ?? resource;
    }

    private async Task SetProgressAsync(
        GeoResourceEntity resource,
        GeoResourceStatus status,
        string message,
        CancellationToken cancellationToken)
    {
        resource.Status = status;
        resource.StatusMessage = AppendStatusMessage(resource.StatusMessage, message);
        _ = await geoResources.UpdateAsync(resource, cancellationToken);
    }

    private async Task CompleteOperationAsync(
        GeoResourceEntity resource,
        CancellationToken cancellationToken)
    {
        resource.Status = GeoResourceStatus.Success;
        resource.StatusMessage = null;
        resource.LastErrorAt = null;
        _ = await geoResources.UpdateAsync(resource, cancellationToken);
        await RestartCoreIfRunningAsync(resource.Node, cancellationToken);
    }

    private async Task FailOperationAsync(
        GeoResourceEntity resource,
        Exception exception,
        CancellationToken cancellationToken)
    {
        resource.Status = GeoResourceStatus.Error;
        resource.StatusMessage = AppendStatusMessage(resource.StatusMessage, $"Error: {exception.Message}");
        resource.LastErrorAt = DateTimeOffset.UtcNow;
        _ = await geoResources.UpdateAsync(resource, cancellationToken);
    }

    private async Task ScheduleOperationOrFailAsync(
        GeoResourceEntity resource,
        long geoResourceId,
        GeoResourceOperation operation,
        string? uploadFilePath,
        string? previousFileName,
        CancellationToken cancellationToken)
    {
        try
        {
            await scheduler.ScheduleGeoResourceOperation(
                geoResourceId,
                operation,
                uploadFilePath,
                previousFileName,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            DeleteTemporaryUpload(uploadFilePath);
            await FailOperationAsync(resource, exception, cancellationToken);
            throw;
        }
    }

    private static void EnsureAvailable(GeoResourceEntity resource)
    {
        if (resource.Status != GeoResourceStatus.Success)
        {
            throw new NodeGeoResourceValidationException("Geo resource file is not available yet.");
        }
    }

    private static string AppendStatusMessage(string? current, string message)
    {
        var timestampedMessage = $"[{DateTimeOffset.UtcNow:O}] {message}";

        return string.IsNullOrWhiteSpace(current)
            ? timestampedMessage
            : $"{current}{Environment.NewLine}{timestampedMessage}";
    }

    private async Task<GeoResourceEntity> GetResourceAsync(
        Guid adminId,
        long nodeId,
        long id,
        CancellationToken cancellationToken)
    {
        return await geoResources.GetByIdAsync(adminId, nodeId, id, cancellationToken)
            ?? throw new NodeGeoResourceNotFoundException($"Geo resource '{id}' was not found.");
    }

    private async Task EnsureUniqueAsync(
        Guid adminId,
        long nodeId,
        string fileName,
        long? currentId,
        CancellationToken cancellationToken)
    {
        var existing = await geoResources.GetByFilenameAsync(adminId, nodeId, fileName, cancellationToken);
        if (existing is not null && existing.Id != currentId)
        {
            throw new NodeGeoResourceConflictException($"Geo resource file '{fileName}' already exists.");
        }
    }

    private async Task<MemoryStream> DownloadAsync(string url, CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient(GeoDownloadClientName);
        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var memory = new MemoryStream();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await stream.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        return memory;
    }

    private static async Task<string> SaveUploadAsync(Stream content, CancellationToken cancellationToken)
    {
        var directory = Path.Combine(PathProvider.Paths.DownloadsDirectory, "geo-resources");
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, $"{Guid.NewGuid():N}.upload");
        await using var file = File.Create(path);
        await content.CopyToAsync(file, cancellationToken);

        return path;
    }

    private void DeleteTemporaryUpload(string? uploadFilePath)
    {
        if (string.IsNullOrWhiteSpace(uploadFilePath))
        {
            return;
        }

        try
        {
            if (File.Exists(uploadFilePath))
            {
                File.Delete(uploadFilePath);
            }
        }
        catch (IOException)
        {
            logger.LogWarning("Failed to delete temporary geo resource upload {UploadFilePath}.", uploadFilePath);
        }
        catch (UnauthorizedAccessException)
        {
            logger.LogWarning("Access denied while deleting temporary geo resource upload {UploadFilePath}.", uploadFilePath);
        }
    }

    private async Task RestartCoreIfRunningAsync(NodeEntity node, CancellationToken cancellationToken)
    {
        if (!coreStateStore.TryGet(node.Id, out var state) || state?.IsRunning != true)
        {
            return;
        }

        await CreateCoreClient(node).RestartCoreAsync(
            coreConfigBuilder.Build(node),
            cancellationToken);
    }

    private INodeGeoResourceClient CreateClient(NodeEntity node)
    {
        return geoResourceClientFactory.Create(new NodeEndpoint(
            node.Id,
            node.Address,
            node.ApiPort,
            secrets.UnprotectApiKey(node.EncryptedApiKey)));
    }

    private INodeCoreClient CreateCoreClient(NodeEntity node)
    {
        return coreClientFactory.Create(new NodeEndpoint(
            node.Id,
            node.Address,
            node.ApiPort,
            secrets.UnprotectApiKey(node.EncryptedApiKey)));
    }

    private static string NormalizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new NodeGeoResourceValidationException("Geo resource file name is required.");
        }

        var normalized = fileName.Trim();
        if (normalized.Length > MaxFilenameLength)
        {
            throw new NodeGeoResourceValidationException($"Geo resource file name must be {MaxFilenameLength} characters or fewer.");
        }

        if (!string.Equals(Path.GetFileName(normalized), normalized, StringComparison.Ordinal)
            || normalized.Contains('/', StringComparison.Ordinal)
            || normalized.Contains('\\', StringComparison.Ordinal)
            || normalized.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new NodeGeoResourceValidationException("Geo resource file name is invalid.");
        }

        return normalized;
    }

    private static string NormalizeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new NodeGeoResourceValidationException("Geo resource URL is required.");
        }

        var normalized = url.Trim();
        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https"))
        {
            throw new NodeGeoResourceValidationException("Geo resource URL must be an absolute HTTP or HTTPS URL.");
        }

        return normalized;
    }

    private static string NormalizeCron(string? cronTemplate)
    {
        if (string.IsNullOrWhiteSpace(cronTemplate))
        {
            throw new NodeGeoResourceValidationException("Geo resource cron template is required.");
        }

        var normalized = cronTemplate.Trim();
        _ = ToQuartzCron(normalized);

        return normalized;
    }

    private static DateTimeOffset GetNextRun(string cronTemplate, DateTimeOffset after)
    {
        var expression = new CronExpression(ToQuartzCron(cronTemplate));

        return expression.GetNextValidTimeAfter(after)
            ?? throw new NodeGeoResourceValidationException("Geo resource cron template does not produce future runs.");
    }

    private static string ToQuartzCron(string unixCron)
    {
        var parts = unixCron.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 5)
        {
            throw new NodeGeoResourceValidationException("Geo resource cron template must use five Unix cron fields.");
        }

        var dayOfMonth = parts[2];
        var dayOfWeek = parts[4];
        if (dayOfMonth != "*" && dayOfWeek != "*")
        {
            throw new NodeGeoResourceValidationException("Geo resource cron template cannot constrain both day-of-month and day-of-week.");
        }

        var quartz = dayOfMonth == "*"
            ? $"0 {parts[0]} {parts[1]} ? {parts[3]} {dayOfWeek}"
            : $"0 {parts[0]} {parts[1]} {dayOfMonth} {parts[3]} ?";

        try
        {
            _ = new CronExpression(quartz);
        }
        catch (FormatException exception)
        {
            throw new NodeGeoResourceValidationException($"Geo resource cron template is invalid. {exception.Message}");
        }

        return quartz;
    }
}
