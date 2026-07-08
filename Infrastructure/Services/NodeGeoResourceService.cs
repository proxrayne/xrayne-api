using System.Net;
using Contracts.Utilities;
using Contracts.Values;
using Data.Contracts;
using Data.Entities;
using Infrastructure.Dto;
using Quartz;
using RemoteNode.Exceptions;
using RemoteNode.Models;
using RemoteNode.Services;

namespace Infrastructure.Services;

/// <summary>
/// Manages geo resource metadata and remote node file synchronization.
/// </summary>
public sealed class NodeGeoResourceService(
    IGeoResourceRepository geoResources,
    INodeSecretService secrets,
    IRemoteNodeApiClientFactory apiClientFactory,
    IRemoteNodeCoreStateStore coreStateStore,
    INodeCoreConfigBuilder coreConfigBuilder,
    IHttpClientFactory httpClientFactory) : INodeGeoResourceService
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

        foreach (var stale in existing.Where(resource => !remoteByName.ContainsKey(resource.Filename)).ToList())
        {
            _ = await geoResources.DeleteAsync(adminId, stale.Id, cancellationToken);
        }

        foreach (var remoteResource in remote)
        {
            _ = await UpsertFromRemoteAsync(
                adminId,
                node,
                remoteResource,
                GeoResourceSourceTypes.Static,
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
    public async Task<GeoResourceEntity> CreateFileAsync(
        Guid adminId,
        NodeEntity node,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var normalizedFileName = NormalizeFileName(fileName);
        await EnsureUniqueAsync(adminId, node.Id, normalizedFileName, null, cancellationToken);

        var remote = await CreateClient(node).UploadGeoResourceAsync(normalizedFileName, content, cancellationToken);
        var created = await UpsertFromRemoteAsync(
            adminId,
            node,
            remote,
            GeoResourceSourceTypes.Static,
            null,
            null,
            null,
            preserveExistingMetadata: false,
            cancellationToken: cancellationToken);
        await RestartCoreIfRunningAsync(node, cancellationToken);

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

        await using var content = await DownloadAsync(normalizedUrl, cancellationToken);
        var remote = await CreateClient(node).UploadGeoResourceAsync(normalizedFileName, content, cancellationToken);
        var created = await UpsertFromRemoteAsync(
            adminId,
            node,
            remote,
            GeoResourceSourceTypes.AutoUpdate,
            normalizedUrl,
            normalizedCron,
            GetNextRun(normalizedCron, DateTimeOffset.UtcNow),
            preserveExistingMetadata: false,
            cancellationToken: cancellationToken);
        await RestartCoreIfRunningAsync(node, cancellationToken);

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

        var isAutoUpdate = resource.SourceType == GeoResourceSourceTypes.AutoUpdate;
        var nextUrl = isAutoUpdate ? NormalizeUrl(url) : null;
        var nextCron = isAutoUpdate ? NormalizeCron(cronTemplate) : null;
        var fileNameChanged = !string.Equals(resource.Filename, nextFileName, StringComparison.Ordinal);
        var refreshRequired = isAutoUpdate
            && (!string.Equals(resource.Url, nextUrl, StringComparison.Ordinal)
                || !string.Equals(resource.CronTemplate, nextCron, StringComparison.Ordinal));
        var client = CreateClient(node);
        var remote = fileNameChanged
            ? await client.RenameGeoResourceAsync(resource.Filename, new RenameGeoResourceRequest(nextFileName), cancellationToken)
            : new GeoResourceDto(resource.Filename, resource.SizeBytes, resource.LastModifiedAt);

        if (refreshRequired)
        {
            await using var content = await DownloadAsync(nextUrl!, cancellationToken);
            remote = await client.UploadGeoResourceAsync(nextFileName, content, cancellationToken);
        }

        resource.Filename = NormalizeFileName(remote.FileName);
        resource.SizeBytes = remote.SizeBytes;
        resource.LastModifiedAt = remote.LastModifiedAt;
        resource.Url = nextUrl;
        resource.CronTemplate = nextCron;
        resource.NextRunAt = isAutoUpdate
            ? refreshRequired && nextCron is not null
                ? GetNextRun(nextCron, DateTimeOffset.UtcNow)
                : resource.NextRunAt
            : null;
        resource.LastError = null;
        resource.LastErrorAt = null;

        var updated = await geoResources.UpdateAsync(adminId, resource, cancellationToken)
            ?? throw new NodeGeoResourceNotFoundException($"Geo resource '{id}' was not found.");
        await RestartCoreIfRunningAsync(node, cancellationToken);

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
        catch (RemoteNodeHttpException exception) when (exception.StatusCode is HttpStatusCode.NotFound)
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

        return await CreateClient(node).DownloadGeoResourceAsync(resource.Filename, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RefreshDueAutoUpdatesAsync(CancellationToken cancellationToken = default)
    {
        var due = await geoResources.GetDueAutoUpdateAsync(DateTimeOffset.UtcNow, cancellationToken);
        foreach (var resource in due)
        {
            await RefreshAutoUpdateAsync(resource, cancellationToken);
        }
    }

    private async Task RefreshAutoUpdateAsync(
        GeoResourceEntity resource,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(resource.Url) || string.IsNullOrWhiteSpace(resource.CronTemplate))
            {
                throw new NodeGeoResourceValidationException("Auto-updated geo resource is missing URL or cron template.");
            }

            await using var content = await DownloadAsync(resource.Url, cancellationToken);
            var remote = await CreateClient(resource.Node).UploadGeoResourceAsync(
                resource.Filename,
                content,
                cancellationToken);

            resource.SizeBytes = remote.SizeBytes;
            resource.LastModifiedAt = remote.LastModifiedAt;
            resource.NextRunAt = GetNextRun(resource.CronTemplate, DateTimeOffset.UtcNow);
            resource.LastError = null;
            resource.LastErrorAt = null;
            _ = await geoResources.UpdateAsync(resource, cancellationToken);
            await RestartCoreIfRunningAsync(resource.Node, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            resource.LastError = exception.Message;
            resource.LastErrorAt = DateTimeOffset.UtcNow;
            _ = await geoResources.UpdateAsync(resource, cancellationToken);
        }
    }

    private async Task<GeoResourceEntity> UpsertFromRemoteAsync(
        Guid adminId,
        NodeEntity node,
        GeoResourceDto remoteResource,
        string sourceType,
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
                SourceType = sourceType,
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
            resource.SourceType = sourceType;
            resource.Url = url;
            resource.CronTemplate = cronTemplate;
            resource.NextRunAt = nextRunAt;
            resource.LastError = null;
            resource.LastErrorAt = null;
        }

        return await geoResources.UpdateAsync(adminId, resource, cancellationToken)
            ?? resource;
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

    private async Task RestartCoreIfRunningAsync(NodeEntity node, CancellationToken cancellationToken)
    {
        if (!coreStateStore.TryGet(node.Id, out var state) || state?.IsRunning != true)
        {
            return;
        }

        await CreateClient(node).RestartCoreAsync(
            new StartCoreRequest(XrayJsonSerializer.Serialize(coreConfigBuilder.Build(node))),
            cancellationToken);
    }

    private IRemoteNodeApiClient CreateClient(NodeEntity node)
    {
        return apiClientFactory.Create(new RemoteNodeEndpoint(
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
