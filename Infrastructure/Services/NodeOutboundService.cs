using System.Text.Json;
using Contracts.Utilities;
using Data.Contracts;
using Data.Entities;
using Infrastructure.Dto;
using RemoteNode.Models;
using RemoteNode.Services;
using Xray.Config.Models;

namespace Infrastructure.Services;

/// <summary>
/// Manages node outbound persistence, validation, and remote runtime synchronization.
/// </summary>
public sealed class NodeOutboundService(
    INodeRepository nodes,
    IOutboundRepository outbounds,
    INodeSecretService secrets,
    IRemoteNodeApiClientFactory apiClientFactory,
    IRemoteNodeCoreStateStore coreStateStore) : INodeOutboundService
{
    /// <inheritdoc />
    public async Task<List<OutboundEntity>> GetByNodeIdAsync(
        long nodeId,
        CancellationToken cancellationToken = default)
    {
        _ = await GetNodeAsync(nodeId, cancellationToken);

        return await outbounds.GetByNodeIdAsync(nodeId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OutboundEntity> GetByNodeAndIdAsync(
        long nodeId,
        long outboundId,
        CancellationToken cancellationToken = default)
    {
        _ = await GetNodeAsync(nodeId, cancellationToken);

        return await GetOutboundAsync(nodeId, outboundId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OutboundEntity> CreateAsync(
        Guid adminId,
        long nodeId,
        string config,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        var outboundConfig = ParseOutbound(config);
        var existing = await outbounds.GetByNodeIdAsync(nodeId, cancellationToken);
        ValidateOutbound(outboundConfig, existing, null, enabled, allowDisabledReadonlyConflicts: false);

        var outbound = new OutboundEntity
        {
            Enabled = enabled,
            ReadOnly = false,
            Config = outboundConfig
        };

        var created = await outbounds.AddAsync(adminId, node.Id, outbound, cancellationToken);
        if (created.Enabled)
        {
            await SyncRemoteAddAsync(node, created, cancellationToken);
        }

        return created;
    }

    /// <inheritdoc />
    public async Task<OutboundEntity> UpdateAsync(
        long nodeId,
        long outboundId,
        string config,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        var outbound = await GetOutboundAsync(nodeId, outboundId, cancellationToken);
        if (outbound.ReadOnly)
        {
            throw new NodeOutboundReadonlyException("Readonly outbounds can only be enabled or disabled.");
        }

        var oldTag = outbound.Tag!;
        var wasEnabled = outbound.Enabled;
        var outboundConfig = ParseOutbound(config);
        var existing = await outbounds.GetByNodeIdAsync(nodeId, cancellationToken);
        ValidateOutbound(outboundConfig, existing, outbound.Id, enabled, allowDisabledReadonlyConflicts: false);

        outbound.Config = outboundConfig;
        outbound.Enabled = enabled;

        var updated = await outbounds.UpdateAsync(outbound, cancellationToken);
        if (updated is null)
        {
            throw new NodeOutboundNotFoundException($"Outbound '{outboundId}' was not found.");
        }

        if (updated.Enabled)
        {
            await SyncRemoteUpdateAsync(node, oldTag, updated, cancellationToken);
        }
        else if (wasEnabled)
        {
            await SyncRemoteDeleteAsync(node, oldTag, cancellationToken);
        }

        return updated;
    }

    /// <inheritdoc />
    public async Task<OutboundEntity> UpdateEnabledAsync(
        long nodeId,
        long outboundId,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        var outbound = await GetOutboundAsync(nodeId, outboundId, cancellationToken);
        if (outbound.Enabled == enabled)
        {
            return outbound;
        }

        var existing = await outbounds.GetByNodeIdAsync(nodeId, cancellationToken);
        ValidateOutbound(outbound.Config, existing, outbound.Id, enabled, allowDisabledReadonlyConflicts: outbound.ReadOnly);

        outbound.Enabled = enabled;
        var updated = await outbounds.UpdateAsync(outbound, cancellationToken);
        if (updated is null)
        {
            throw new NodeOutboundNotFoundException($"Outbound '{outboundId}' was not found.");
        }

        if (updated.Enabled)
        {
            await SyncRemoteAddAsync(node, updated, cancellationToken);
        }
        else
        {
            await SyncRemoteDeleteAsync(node, updated, cancellationToken);
        }

        return updated;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(long nodeId, long outboundId, CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        var outbound = await GetOutboundAsync(nodeId, outboundId, cancellationToken);
        if (outbound.ReadOnly)
        {
            throw new NodeOutboundReadonlyException("Readonly outbounds are managed through the node config template.");
        }

        var outboundTag = outbound.Tag!;
        var wasEnabled = outbound.Enabled;
        var deleted = await outbounds.DeleteAsync(outbound.Id, cancellationToken);
        if (!deleted)
        {
            throw new NodeOutboundNotFoundException($"Outbound '{outboundId}' was not found.");
        }

        if (wasEnabled)
        {
            await SyncRemoteDeleteAsync(node, outboundTag, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task SyncReadonlyFromTemplateAsync(
        Guid adminId,
        NodeEntity node,
        XrayConfig template,
        CancellationToken cancellationToken = default)
    {
        var desired = (template.Outbounds ?? [])
            .Select(NormalizeOutboundTag)
            .Where(outbound => !string.IsNullOrWhiteSpace(outbound.Tag))
            .GroupBy(outbound => outbound.Tag, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();
        var existing = await outbounds.GetByNodeIdAsync(node.Id, cancellationToken);
        var readonlyByTag = existing
            .Where(outbound => outbound.ReadOnly && !string.IsNullOrWhiteSpace(outbound.Tag))
            .ToDictionary(outbound => outbound.Tag!, StringComparer.Ordinal);
        var desiredTags = desired.Select(outbound => outbound.Tag!).ToHashSet(StringComparer.Ordinal);

        foreach (var stale in readonlyByTag.Values.Where(outbound => !desiredTags.Contains(outbound.Tag!)).ToList())
        {
            var wasEnabled = stale.Enabled;
            await outbounds.DeleteAsync(stale.Id, cancellationToken);
            if (wasEnabled)
            {
                await SyncRemoteDeleteAsync(node, stale, cancellationToken);
            }
        }

        var current = await outbounds.GetByNodeIdAsync(node.Id, cancellationToken);
        foreach (var outboundConfig in desired)
        {
            var existingReadonly = current
                .FirstOrDefault(outbound => outbound.ReadOnly
                    && string.Equals(outbound.Tag, outboundConfig.Tag, StringComparison.Ordinal));
            var valid = IsValidReadonlyTemplateOutbound(outboundConfig, current, existingReadonly?.Id);
            var enabled = existingReadonly is null
                ? valid
                : existingReadonly.Enabled && valid;

            if (existingReadonly is null)
            {
                if (!valid)
                {
                    continue;
                }

                var created = await outbounds.AddAsync(
                    adminId,
                    node.Id,
                    new OutboundEntity
                    {
                        Enabled = enabled,
                        ReadOnly = true,
                        Config = outboundConfig
                    },
                    cancellationToken);
                current.Add(created);
                if (created.Enabled)
                {
                    await SyncRemoteAddAsync(node, created, cancellationToken);
                }

                continue;
            }

            var wasEnabled = existingReadonly.Enabled;

            existingReadonly.Config = outboundConfig;
            existingReadonly.Enabled = enabled;

            var updated = await outbounds.UpdateAsync(existingReadonly, cancellationToken);
            if (updated is null)
            {
                throw new NodeOutboundNotFoundException($"Outbound '{existingReadonly.Id}' was not found.");
            }

            if (updated.Enabled)
            {
                await SyncRemoteUpdateAsync(node, updated, cancellationToken);
            }
            else if (wasEnabled)
            {
                await SyncRemoteDeleteAsync(node, existingReadonly, cancellationToken);
            }
        }
    }

    private static Outbound ParseOutbound(string config)
    {
        if (string.IsNullOrWhiteSpace(config))
        {
            throw new NodeOutboundValidationException("Outbound configuration is required.");
        }

        try
        {
            var outbound = XrayJsonSerializer.Deserialize<Outbound>(config);
            if (outbound is null)
            {
                throw new NodeOutboundValidationException("Outbound configuration is invalid.");
            }

            if (string.IsNullOrWhiteSpace(outbound.Tag))
            {
                throw new NodeOutboundValidationException("Outbound tag is required.");
            }

            return NormalizeOutboundTag(outbound);
        }
        catch (JsonException exception)
        {
            throw new NodeOutboundValidationException($"Outbound configuration is invalid. {exception.Message}");
        }
        catch (InvalidOperationException exception) when (exception is not NodeOutboundValidationException)
        {
            throw new NodeOutboundValidationException($"Outbound configuration is invalid. {exception.Message}");
        }
    }

    private static void ValidateOutbound(
        Outbound config,
        IEnumerable<OutboundEntity> existing,
        long? currentId,
        bool enabled,
        bool allowDisabledReadonlyConflicts)
    {
        if (string.IsNullOrWhiteSpace(config.Tag))
        {
            throw new NodeOutboundValidationException("Outbound tag is required.");
        }

        foreach (var outbound in existing.Where(outbound => outbound.Id != currentId))
        {
            if (string.Equals(outbound.Tag, config.Tag, StringComparison.Ordinal))
            {
                throw new NodeOutboundConflictException($"Outbound tag '{config.Tag}' already exists on this node.");
            }

            var conflictsWithReadonly = outbound.ReadOnly && !outbound.Enabled;
            if (allowDisabledReadonlyConflicts && conflictsWithReadonly)
            {
                continue;
            }
        }
    }

    private static bool IsValidReadonlyTemplateOutbound(
        Outbound config,
        IEnumerable<OutboundEntity> existing,
        long? currentId)
    {
        return existing
            .Where(outbound => outbound.Id != currentId)
            .All(outbound =>
            {
                if (outbound.ReadOnly && !outbound.Enabled)
                {
                    return !string.Equals(outbound.Tag, config.Tag, StringComparison.Ordinal);
                }

                return !string.Equals(outbound.Tag, config.Tag, StringComparison.Ordinal);
            });
    }

    private static Outbound NormalizeOutboundTag(Outbound outbound)
    {
        if (!string.IsNullOrWhiteSpace(outbound.Tag))
        {
            outbound.Tag = outbound.Tag.Trim();
        }

        return outbound;
    }

    private async Task<NodeEntity> GetNodeAsync(long nodeId, CancellationToken cancellationToken)
    {
        var node = await nodes.GetByIdAsync(nodeId, cancellationToken);
        if (node is null)
        {
            throw new NodeOutboundNotFoundException($"Node '{nodeId}' was not found.");
        }

        return node;
    }

    private async Task<OutboundEntity> GetOutboundAsync(
        long nodeId,
        long outboundId,
        CancellationToken cancellationToken)
    {
        var outbound = await outbounds.GetByNodeAndIdAsync(nodeId, outboundId, cancellationToken);
        if (outbound is null)
        {
            throw new NodeOutboundNotFoundException($"Outbound '{outboundId}' was not found.");
        }

        return outbound;
    }

    private async Task SyncRemoteAddAsync(
        NodeEntity node,
        OutboundEntity outbound,
        CancellationToken cancellationToken)
    {
        if (!IsRemoteCoreRunning(node.Id))
        {
            return;
        }

        await CreateRemoteNodeClient(node).AddOutboundAsync(
            CreateSyncOutboundRequest(outbound),
            cancellationToken);
    }

    private async Task SyncRemoteUpdateAsync(
        NodeEntity node,
        OutboundEntity outbound,
        CancellationToken cancellationToken)
    {
        await SyncRemoteUpdateAsync(node, outbound.Tag!, outbound, cancellationToken);
    }

    private async Task SyncRemoteUpdateAsync(
        NodeEntity node,
        string oldTag,
        OutboundEntity outbound,
        CancellationToken cancellationToken)
    {
        if (!IsRemoteCoreRunning(node.Id))
        {
            return;
        }

        await CreateRemoteNodeClient(node).UpdateOutboundAsync(
            oldTag,
            CreateSyncOutboundRequest(outbound),
            cancellationToken);
    }

    private async Task SyncRemoteDeleteAsync(
        NodeEntity node,
        string tag,
        CancellationToken cancellationToken)
    {
        if (!IsRemoteCoreRunning(node.Id))
        {
            return;
        }

        await CreateRemoteNodeClient(node).DeleteOutboundAsync(tag, cancellationToken);
    }

    private async Task SyncRemoteDeleteAsync(
        NodeEntity node,
        OutboundEntity outbound,
        CancellationToken cancellationToken)
    {
        if (!IsRemoteCoreRunning(node.Id))
        {
            return;
        }

        await CreateRemoteNodeClient(node).DeleteOutboundAsync(outbound.Tag!, cancellationToken);
    }

    private static SyncOutboundRequest CreateSyncOutboundRequest(OutboundEntity outbound)
    {
        return new SyncOutboundRequest
        {
            Outbound = outbound.Config
        };
    }

    private bool IsRemoteCoreRunning(long nodeId)
        => coreStateStore.TryGet(nodeId, out var state) && state?.IsRunning == true;

    private IRemoteNodeApiClient CreateRemoteNodeClient(NodeEntity node)
    {
        return apiClientFactory.Create(new RemoteNodeEndpoint(
            node.Id,
            node.Address,
            node.ApiPort,
            secrets.UnprotectApiKey(node.EncryptedApiKey)));
    }
}
