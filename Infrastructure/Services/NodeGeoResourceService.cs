using System.ComponentModel.DataAnnotations;
using System.Net;
using Contracts.Enums;
using Contracts.Exceptions;
using Data.Contracts;
using Data.Entities;
using Infrastructure.Dto;
using Infrastructure.Utilities;
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
    IHttpClientFactory httpClientFactory,
    IBackgroundTaskScheduler scheduler,
    INodeCoreService coreService,
    ITempFileStorage fileStorage) : INodeGeoResourceService
{
    private const int MaxFilenameLength = 128;
    private const string GeoDownloadClientName = "geo-resources";

    /// <inheritdoc />
    public async Task SynchronizeNodeAsync(
        NodeEntity node,
        CancellationToken ct = default)
    {
        var remote = await CreateClient(node).GetGeoResourcesAsync(ct);
        var existing = await geoResources.GetAllAsync(node.Admin.Id, node.Id, ct);
        var remoteByName = remote.ToDictionary(
            resource => StringNormalizer.NormalizeFileName(resource.FileName, MaxFilenameLength),
            StringComparer.OrdinalIgnoreCase);

        foreach (var stale in existing.Where(resource => resource.Status == GeoResourceStatus.Success && !remoteByName.ContainsKey(resource.Filename)))
        {
            await geoResources.DeleteAsync(stale.Id, ct);
        }

        foreach (var remoteResource in remote)
        {
            await UpsertFromRemoteAsync(node, remoteResource, ct);
        }
    }

    /// <inheritdoc />
    public async Task<GeoResourceEntity> CreateFileAsync(
        long adminId,
        NodeEntity node,
        string fileName,
        Stream content,
        CancellationToken ct = default)
    {
        var normalizedFileName = StringNormalizer.NormalizeFileName(fileName, MaxFilenameLength);

        await EnsureUniqueAsync(node.Id, normalizedFileName, null, ct);

        var filepath = await fileStorage.WriteAsync(content, ct);
        var created = await geoResources.AddAsync(new GeoResourceEntity
        {
            Filename = normalizedFileName,
            SizeBytes = 0,
            LastModifiedAt = DateTimeOffset.UtcNow,
            Status = GeoResourceStatus.Queued,
            StatusMessage = "Queued uploaded file transfer.",
            NodeId = node.Id,
            Node = node,
            AdminId = adminId,
            Admin = node.Admin
        }, ct);

        await scheduler.ScheduleGeoResourceUpload(created.Id, filepath, ct);

        return created;
    }

    /// <inheritdoc />
    public async Task<GeoResourceEntity> CreateAutoUpdateAsync(
        long adminId,
        NodeEntity node,
        string fileName,
        string url,
        int updateInterval,
        CancellationToken ct = default)
    {
        var normalizedFileName = StringNormalizer.NormalizeFileName(fileName, MaxFilenameLength);

        await EnsureUniqueAsync(node.Id, normalizedFileName, null, ct);

        var normalizedUrl = StringNormalizer.NormalizeUrl(url);

        var created = await geoResources.AddAsync(new GeoResourceEntity
        {
            Filename = normalizedFileName,
            SizeBytes = 0,
            LastModifiedAt = DateTimeOffset.UtcNow,
            Status = GeoResourceStatus.Queued,
            StatusMessage = "Queued URL download.",
            Url = normalizedUrl,
            UpdateInterval = updateInterval,
            NodeId = node.Id,
            Node = node,
            AdminId = adminId,
            Admin = node.Admin
        }, ct);

        await ScheduleDownloadAutoUpdatesAsync(created, ct);

        return created;
    }

    /// <inheritdoc />
    public async Task<GeoResourceEntity> UpdateAsync(
        NodeEntity node,
        long id,
        string fileName,
        string? url,
        int? updateInterval,
        CancellationToken ct = default)
    {
        var resource = await geoResources.GetByNodeIdAsync(node.Id, id, ct);
        var nextFileName = StringNormalizer.NormalizeFileName(fileName, MaxFilenameLength);

        await EnsureUniqueAsync(node.Id, nextFileName, resource.Id, ct);

        var isRefreshRequired = false;
        if (resource.IsAutoUpdate)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                var normalizeUrl = StringNormalizer.NormalizeUrl(url);

                isRefreshRequired = !string.Equals(resource.Url, normalizeUrl, StringComparison.Ordinal);

                resource.Url = normalizeUrl;
            }

            if (updateInterval is not null)
            {
                resource.UpdateInterval = updateInterval;
                resource.NextRunAt = resource.LastModifiedAt.AddHours((double)updateInterval);
            }
        }

        if (!string.Equals(resource.Filename, nextFileName, StringComparison.Ordinal))
        {
            await CreateClient(resource.Node).RenameGeoResourceAsync(resource.Filename, nextFileName, ct);
        }

        resource.LastErrorAt = null;
        resource.Status = isRefreshRequired ? GeoResourceStatus.Queued : resource.Status;
        resource.StatusMessage = isRefreshRequired ? "Queued geo resource refresh." : null;
        resource.Filename = nextFileName;

        var updated = await geoResources.UpdateAsync(resource, ct) ?? throw new NotFoundException($"Geo resource '{id}' was not found.");

        if (isRefreshRequired || resource.NextRunAt < DateTimeOffset.UtcNow)
        {
            await ScheduleDownloadAutoUpdatesAsync(resource, ct);
        }

        return updated;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        NodeEntity node,
        long id,
        CancellationToken cancellationToken = default)
    {
        var resource = await geoResources.GetByNodeIdAsync(node.Id, id, cancellationToken);
        try
        {
            await CreateClient(node).DeleteGeoResourceAsync(resource.Filename, cancellationToken);
        }
        catch (NodeHttpException exception) when (exception.StatusCode is HttpStatusCode.NotFound)
        {
        }

        await Task.WhenAll([
            geoResources.DeleteAsync(resource.Id, cancellationToken),
            coreService.RestartCoreAsync(node, cancellationToken)
        ]);
    }

    /// <inheritdoc />
    public async Task<GeoResourceContent> DownloadResourceAsync(
        NodeEntity node,
        long id,
        CancellationToken cancellationToken = default)
    {
        var resource = await geoResources.GetByNodeIdAsync(node.Id, id, cancellationToken);
        if (resource.Status != GeoResourceStatus.Success)
        {
            throw new ValidationException("Geo resource file is not available yet.");
        }

        return await CreateClient(node).DownloadGeoResourceAsync(resource.Filename, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GeoResourceEntity> UploadToNodeAsync(GeoResourceEntity resource, Stream content, CancellationToken ct = default)
    {
        var remote = await CreateClient(resource.Node).UploadGeoResourceAsync(
                          resource.Filename,
                          content,
                          ct);

        resource.SizeBytes = remote.SizeBytes;
        resource.LastModifiedAt = remote.LastModifiedAt;

        if (resource.UpdateInterval != null)
        {
            resource.NextRunAt = DateTimeOffset.UtcNow.AddHours((double)resource.UpdateInterval);
        }

        return await geoResources.UpdateAsync(resource, ct) ?? throw new Exception("Failure update geo resource");
    }

    private async Task UpsertFromRemoteAsync(
        NodeEntity node,
        GeoResourceDto remoteResource,
        CancellationToken cancellationToken)
    {
        var fileName = StringNormalizer.NormalizeFileName(remoteResource.FileName, MaxFilenameLength);
        var resource = await geoResources.GetByFilenameAsync(node.Admin.Id, node.Id, fileName, cancellationToken);
        if (resource is null)
        {
            resource = new GeoResourceEntity
            {
                Filename = fileName,
                SizeBytes = remoteResource.SizeBytes,
                LastModifiedAt = remoteResource.LastModifiedAt,
                Status = GeoResourceStatus.Success,
                NodeId = node.Id,
                Node = node,
                AdminId = node.Admin.Id,
                Admin = node.Admin
            };

            await geoResources.AddAsync(resource, cancellationToken);

            return;
        }

        resource.SizeBytes = remoteResource.SizeBytes;
        resource.LastModifiedAt = remoteResource.LastModifiedAt;
        resource.Status = GeoResourceStatus.Success;
        resource.StatusMessage = null;
        resource.LastErrorAt = null;

        await geoResources.UpdateAsync(node.Admin.Id, resource, cancellationToken);

    }

    /// <inheritdoc/>
    public async Task UpdateStatusAsync(
        GeoResourceEntity resource,
        GeoResourceStatus status,
        string message,
        CancellationToken ct)
    {
        resource.Status = status;

        if (status == GeoResourceStatus.Error)
        {
            resource.LastErrorAt = DateTimeOffset.UtcNow;
        }

        if (status == GeoResourceStatus.Success)
        {
            resource.LastErrorAt = null;
            resource.StatusMessage = null;
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            resource.StatusMessage = AppendStatusMessage(resource.StatusMessage, message);
        }

        _ = await geoResources.UpdateAsync(resource, ct);
    }

    /// <inheritdoc/>
    public Task UpdateStatusAsync(
        GeoResourceEntity resource,
        GeoResourceStatus status,
        CancellationToken ct)
    => UpdateStatusAsync(resource, status, string.Empty, ct);

    public async Task ScheduleDownloadAutoUpdatesAsync(GeoResourceEntity entity, CancellationToken ct = default)
    {
        await UpdateStatusAsync(entity, GeoResourceStatus.Queued, "Queued download & update file.", ct);

        try
        {
            await scheduler.ScheduleGeoResourceDownload(entity.Id, ct);
        }
        catch (Exception ex)
        {
            await UpdateStatusAsync(entity, GeoResourceStatus.Error, $"Error: {ex.Message}", ct);
        }
    }
    private async Task EnsureUniqueAsync(
        long nodeId,
        string fileName,
        long? currentId,
        CancellationToken cancellationToken)
    {
        var existing = await geoResources.GetByFilenameAsync(nodeId, fileName, cancellationToken);
        if (existing is not null && existing.Id != currentId)
        {
            throw new ConflictException($"Geo resource file '{fileName}' already exists.");
        }
    }

    public async Task<MemoryStream> DownloadAsync(string url, CancellationToken cancellationToken)
    {
        using var httpClient = httpClientFactory.CreateClient(GeoDownloadClientName);
        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var memory = new MemoryStream();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await stream.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        return memory;
    }

    private INodeGeoResourceClient CreateClient(NodeEntity node) => geoResourceClientFactory.Create(new NodeEndpoint(
            node.Id,
            node.Address,
            node.ApiPort,
            secrets.UnprotectApiKey(node.EncryptedApiKey)));

    private static string AppendStatusMessage(string? current, string message)
    {
        var timestampedMessage = $"[{DateTimeOffset.UtcNow:O}] {message}";

        return string.IsNullOrWhiteSpace(current)
            ? timestampedMessage
            : $"{current}{Environment.NewLine}{timestampedMessage}";
    }

}
