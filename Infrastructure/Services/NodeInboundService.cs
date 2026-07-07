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
/// Manages node inbound persistence, validation, and remote runtime synchronization.
/// </summary>
public sealed class NodeInboundService(
    INodeRepository nodes,
    IInboundRepository inbounds,
    INodeSecretService secrets,
    IRemoteNodeApiClientFactory apiClientFactory,
    IRemoteNodeCoreStateStore coreStateStore) : INodeInboundService
{
    private const int ReservedXrayApiPort = 10085;

    /// <inheritdoc />
    public async Task<List<InboundEntity>> GetByNodeIdAsync(
        long nodeId,
        CancellationToken cancellationToken = default)
    {
        _ = await GetNodeAsync(nodeId, cancellationToken);

        return await inbounds.GetByNodeIdAsync(nodeId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<InboundEntity> GetByNodeAndIdAsync(
        long nodeId,
        int inboundId,
        CancellationToken cancellationToken = default)
    {
        _ = await GetNodeAsync(nodeId, cancellationToken);

        return await GetInboundAsync(nodeId, inboundId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<InboundEntity> CreateAsync(
        Guid adminId,
        long nodeId,
        string config,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        var inboundConfig = ParseInbound(config);
        var existing = await inbounds.GetByNodeIdAsync(nodeId, cancellationToken);
        ValidateInbound(inboundConfig, existing, null, enabled, allowDisabledReadonlyConflicts: false);

        var inbound = new InboundEntity
        {
            Enabled = enabled,
            ReadOnly = false,
            Config = inboundConfig
        };

        var created = await inbounds.AddAsync(adminId, node.Id, inbound, cancellationToken);
        if (created.Enabled)
        {
            await SyncRemoteAddAsync(node, created, cancellationToken);
        }

        return created;
    }

    /// <inheritdoc />
    public async Task<InboundEntity> UpdateAsync(
        long nodeId,
        int inboundId,
        string config,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        var inbound = await GetInboundAsync(nodeId, inboundId, cancellationToken);
        if (inbound.ReadOnly)
        {
            throw new NodeInboundReadonlyException("Readonly inbounds can only be enabled or disabled.");
        }

        var previousTag = inbound.Tag;
        var wasEnabled = inbound.Enabled;
        var inboundConfig = ParseInbound(config);
        var existing = await inbounds.GetByNodeIdAsync(nodeId, cancellationToken);
        ValidateInbound(inboundConfig, existing, inbound.Id, enabled, allowDisabledReadonlyConflicts: false);

        inbound.Config = inboundConfig;
        inbound.Enabled = enabled;

        var updated = await inbounds.UpdateAsync(inbound, cancellationToken)
            ?? throw new NodeInboundNotFoundException($"Inbound '{inboundId}' was not found.");

        if (updated.Enabled)
        {
            await SyncRemoteUpdateAsync(node, previousTag, updated, cancellationToken);
        }
        else if (wasEnabled)
        {
            await SyncRemoteDeleteAsync(node, previousTag, cancellationToken);
        }

        return updated;
    }

    /// <inheritdoc />
    public async Task<InboundEntity> UpdateEnabledAsync(
        long nodeId,
        int inboundId,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        var inbound = await GetInboundAsync(nodeId, inboundId, cancellationToken);
        if (inbound.Enabled == enabled)
        {
            return inbound;
        }

        var existing = await inbounds.GetByNodeIdAsync(nodeId, cancellationToken);
        ValidateInbound(inbound.Config, existing, inbound.Id, enabled, allowDisabledReadonlyConflicts: inbound.ReadOnly);

        inbound.Enabled = enabled;
        var updated = await inbounds.UpdateAsync(inbound, cancellationToken)
            ?? throw new NodeInboundNotFoundException($"Inbound '{inboundId}' was not found.");

        if (updated.Enabled)
        {
            await SyncRemoteAddAsync(node, updated, cancellationToken);
        }
        else
        {
            await SyncRemoteDeleteAsync(node, updated.Tag, cancellationToken);
        }

        return updated;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(long nodeId, int inboundId, CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        var inbound = await GetInboundAsync(nodeId, inboundId, cancellationToken);
        if (inbound.ReadOnly)
        {
            throw new NodeInboundReadonlyException("Readonly inbounds are managed through the node config template.");
        }

        var tag = inbound.Tag;
        var wasEnabled = inbound.Enabled;
        var deleted = await inbounds.DeleteAsync(inbound.Id, cancellationToken);
        if (!deleted)
        {
            throw new NodeInboundNotFoundException($"Inbound '{inboundId}' was not found.");
        }

        if (wasEnabled)
        {
            await SyncRemoteDeleteAsync(node, tag, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task SyncReadonlyFromTemplateAsync(
        Guid adminId,
        NodeEntity node,
        XrayConfig template,
        CancellationToken cancellationToken = default)
    {
        var desired = (template.Inbounds ?? [])
            .Where(inbound => !string.IsNullOrWhiteSpace(inbound.Tag))
            .GroupBy(inbound => inbound.Tag, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();
        var existing = await inbounds.GetByNodeIdAsync(node.Id, cancellationToken);
        var readonlyByTag = existing
            .Where(inbound => inbound.ReadOnly)
            .ToDictionary(inbound => inbound.Tag, StringComparer.Ordinal);
        var desiredTags = desired.Select(inbound => inbound.Tag).ToHashSet(StringComparer.Ordinal);

        foreach (var stale in readonlyByTag.Values.Where(inbound => !desiredTags.Contains(inbound.Tag)).ToList())
        {
            var tag = stale.Tag;
            var wasEnabled = stale.Enabled;
            await inbounds.DeleteAsync(stale.Id, cancellationToken);
            if (wasEnabled)
            {
                await SyncRemoteDeleteAsync(node, tag, cancellationToken);
            }
        }

        var current = await inbounds.GetByNodeIdAsync(node.Id, cancellationToken);
        foreach (var inboundConfig in desired)
        {
            var existingReadonly = current
                .FirstOrDefault(inbound => inbound.ReadOnly
                    && string.Equals(inbound.Tag, inboundConfig.Tag, StringComparison.Ordinal));
            var valid = IsValidReadonlyTemplateInbound(inboundConfig, current, existingReadonly?.Id);
            var enabled = existingReadonly is null
                ? valid
                : existingReadonly.Enabled && valid;

            if (existingReadonly is null)
            {
                var created = await inbounds.AddAsync(
                    adminId,
                    node.Id,
                    new InboundEntity
                    {
                        Enabled = enabled,
                        ReadOnly = true,
                        Config = inboundConfig
                    },
                    cancellationToken);
                current.Add(created);
                if (created.Enabled)
                {
                    await SyncRemoteAddAsync(node, created, cancellationToken);
                }

                continue;
            }

            var previousTag = existingReadonly.Tag;
            var wasEnabled = existingReadonly.Enabled;

            existingReadonly.Config = inboundConfig;
            existingReadonly.Enabled = enabled;

            var updated = await inbounds.UpdateAsync(existingReadonly, cancellationToken)
                ?? throw new NodeInboundNotFoundException($"Inbound '{existingReadonly.Id}' was not found.");
            if (updated.Enabled)
            {
                await SyncRemoteUpdateAsync(node, previousTag, updated, cancellationToken);
            }
            else if (wasEnabled)
            {
                await SyncRemoteDeleteAsync(node, previousTag, cancellationToken);
            }
        }
    }

    private static Inbound ParseInbound(string config)
    {
        if (string.IsNullOrWhiteSpace(config))
        {
            throw new NodeInboundValidationException("Inbound configuration is required.");
        }

        try
        {
            var inbound = XrayJsonSerializer.Deserialize<Inbound>(config)
                ?? throw new NodeInboundValidationException("Inbound configuration is invalid.");
            if (string.IsNullOrWhiteSpace(inbound.Tag))
            {
                throw new NodeInboundValidationException("Inbound tag is required.");
            }

            return inbound;
        }
        catch (JsonException exception)
        {
            throw new NodeInboundValidationException($"Inbound configuration is invalid. {exception.Message}");
        }
        catch (InvalidOperationException exception) when (exception is not NodeInboundValidationException)
        {
            throw new NodeInboundValidationException($"Inbound configuration is invalid. {exception.Message}");
        }
    }

    private static void ValidateInbound(
        Inbound config,
        IEnumerable<InboundEntity> existing,
        int? currentId,
        bool enabled,
        bool allowDisabledReadonlyConflicts)
    {
        if (IsReservedApiPort(config.Port))
        {
            throw new NodeInboundConflictException($"Port '{ReservedXrayApiPort}' is reserved for the local xray API.");
        }

        if (!enabled && allowDisabledReadonlyConflicts)
        {
            return;
        }

        foreach (var inbound in existing.Where(inbound => inbound.Id != currentId))
        {
            var conflictsWithReadonly = inbound.ReadOnly && !inbound.Enabled;
            if (allowDisabledReadonlyConflicts && conflictsWithReadonly)
            {
                continue;
            }

            if (string.Equals(inbound.Tag, config.Tag, StringComparison.Ordinal))
            {
                throw new NodeInboundConflictException($"Inbound tag '{config.Tag}' already exists on this node.");
            }

            if (string.Equals(inbound.Port.ToString(), config.Port.ToString(), StringComparison.Ordinal))
            {
                throw new NodeInboundConflictException($"Inbound port '{config.Port}' already exists on this node.");
            }
        }
    }

    private static bool IsValidReadonlyTemplateInbound(
        Inbound config,
        IEnumerable<InboundEntity> existing,
        int? currentId)
    {
        if (IsReservedApiPort(config.Port))
        {
            return false;
        }

        return existing
            .Where(inbound => inbound.Id != currentId)
            .All(inbound =>
            {
                if (inbound.ReadOnly && !inbound.Enabled)
                {
                    return true;
                }

                return !string.Equals(inbound.Tag, config.Tag, StringComparison.Ordinal)
                    && !string.Equals(inbound.Port.ToString(), config.Port.ToString(), StringComparison.Ordinal);
            });
    }

    private static bool IsReservedApiPort(Port port)
    {
        if (port.Single == ReservedXrayApiPort)
        {
            return true;
        }

        return port.Range is { } range
            && ReservedXrayApiPort >= range.Item1
            && ReservedXrayApiPort <= range.Item2;
    }

    private async Task<NodeEntity> GetNodeAsync(long nodeId, CancellationToken cancellationToken)
    {
        return await nodes.GetByIdAsync(nodeId, cancellationToken)
            ?? throw new NodeInboundNotFoundException($"Node '{nodeId}' was not found.");
    }

    private async Task<InboundEntity> GetInboundAsync(
        long nodeId,
        int inboundId,
        CancellationToken cancellationToken)
    {
        return await inbounds.GetByNodeAndIdAsync(nodeId, inboundId, cancellationToken)
            ?? throw new NodeInboundNotFoundException($"Inbound '{inboundId}' was not found.");
    }

    private async Task SyncRemoteAddAsync(
        NodeEntity node,
        InboundEntity inbound,
        CancellationToken cancellationToken)
    {
        if (!IsRemoteCoreRunning(node.Id))
        {
            return;
        }

        await CreateRemoteNodeClient(node).AddInboundAsync(
            new SyncInboundRequest(XrayJsonSerializer.Serialize(inbound.Config)),
            cancellationToken);
    }

    private async Task SyncRemoteUpdateAsync(
        NodeEntity node,
        string previousTag,
        InboundEntity inbound,
        CancellationToken cancellationToken)
    {
        if (!IsRemoteCoreRunning(node.Id))
        {
            return;
        }

        await CreateRemoteNodeClient(node).UpdateInboundAsync(
            previousTag,
            new SyncInboundRequest(XrayJsonSerializer.Serialize(inbound.Config)),
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

        await CreateRemoteNodeClient(node).DeleteInboundAsync(tag, cancellationToken);
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
