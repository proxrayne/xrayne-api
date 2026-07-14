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
/// Manages node routing rule persistence and remote runtime synchronization.
/// </summary>
public sealed class NodeRoutingRuleService(
    INodeRepository nodes,
    IRoutingRuleRepository routingRules,
    INodeSecretService secrets,
    IRemoteNodeApiClientFactory apiClientFactory,
    IRemoteNodeCoreStateStore coreStateStore) : INodeRoutingRuleService
{
    private const int PositionStep = 10;

    /// <inheritdoc />
    public async Task<List<RoutingRuleEntity>> GetByNodeIdAsync(
        long nodeId,
        CancellationToken cancellationToken = default)
    {
        _ = await GetNodeAsync(nodeId, cancellationToken);

        return await routingRules.GetByNodeIdAsync(nodeId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RoutingRuleEntity> GetByNodeAndIdAsync(
        long nodeId,
        long routingRuleId,
        CancellationToken cancellationToken = default)
    {
        _ = await GetNodeAsync(nodeId, cancellationToken);

        return await GetRoutingRuleAsync(nodeId, routingRuleId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RoutingRuleEntity> CreateAsync(
        Guid adminId,
        long nodeId,
        string config,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        var ruleConfig = ParseRoutingRule(config);
        var existing = await routingRules.GetByNodeIdAsync(nodeId, cancellationToken);
        ValidateRuleTag(ruleConfig, existing, null);

        var rule = new RoutingRuleEntity
        {
            Enabled = enabled,
            ReadOnly = false,
            Position = GetNextManualPosition(existing),
            Config = ruleConfig
        };

        var created = await routingRules.AddAsync(adminId, node.Id, rule, cancellationToken);
        await SyncRemoteRulesAsync(node, cancellationToken);

        return created;
    }

    /// <inheritdoc />
    public async Task<RoutingRuleEntity> UpdateAsync(
        long nodeId,
        long routingRuleId,
        string config,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        var rule = await GetRoutingRuleAsync(nodeId, routingRuleId, cancellationToken);
        if (rule.ReadOnly)
        {
            throw new NodeRoutingRuleReadonlyException("Readonly routing rules can only be enabled or disabled.");
        }

        var ruleConfig = ParseRoutingRule(config);
        var existing = await routingRules.GetByNodeIdAsync(nodeId, cancellationToken);
        ValidateRuleTag(ruleConfig, existing, rule.Id);

        rule.Config = ruleConfig;
        rule.Enabled = enabled;

        var updated = await routingRules.UpdateAsync(rule, cancellationToken);
        if (updated is null)
        {
            throw new NodeRoutingRuleNotFoundException($"Routing rule '{routingRuleId}' was not found.");
        }

        await SyncRemoteRulesAsync(node, cancellationToken);

        return updated;
    }

    /// <inheritdoc />
    public async Task<RoutingRuleEntity> UpdateEnabledAsync(
        long nodeId,
        long routingRuleId,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        var rule = await GetRoutingRuleAsync(nodeId, routingRuleId, cancellationToken);
        if (rule.Enabled == enabled)
        {
            return rule;
        }

        rule.Enabled = enabled;
        var updated = await routingRules.UpdateAsync(rule, cancellationToken);
        if (updated is null)
        {
            throw new NodeRoutingRuleNotFoundException($"Routing rule '{routingRuleId}' was not found.");
        }

        await SyncRemoteRulesAsync(node, cancellationToken);

        return updated;
    }

    /// <inheritdoc />
    public async Task<List<RoutingRuleEntity>> UpdateOrderAsync(
        long nodeId,
        IReadOnlyList<long> ruleIds,
        CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        var current = await routingRules.GetByNodeIdAsync(nodeId, cancellationToken);
        var manualRules = current.Where(rule => !rule.ReadOnly).ToList();

        ValidateManualOrder(ruleIds, manualRules);
        ApplyManualPositions(current, ruleIds);

        foreach (var rule in current)
        {
            var updated = await routingRules.UpdateAsync(rule, cancellationToken);
            if (updated is null)
            {
                throw new NodeRoutingRuleNotFoundException($"Routing rule '{rule.Id}' was not found.");
            }
        }

        await SyncRemoteRulesAsync(node, cancellationToken);

        return await routingRules.GetByNodeIdAsync(nodeId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        long nodeId,
        long routingRuleId,
        CancellationToken cancellationToken = default)
    {
        var node = await GetNodeAsync(nodeId, cancellationToken);
        var rule = await GetRoutingRuleAsync(nodeId, routingRuleId, cancellationToken);
        if (rule.ReadOnly)
        {
            throw new NodeRoutingRuleReadonlyException("Readonly routing rules are managed through the node config template.");
        }

        var deleted = await routingRules.DeleteAsync(rule.Id, cancellationToken);
        if (!deleted)
        {
            throw new NodeRoutingRuleNotFoundException($"Routing rule '{routingRuleId}' was not found.");
        }

        await ReindexAsync(node.Id, cancellationToken);
        await SyncRemoteRulesAsync(node, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SyncReadonlyFromTemplateAsync(
        Guid adminId,
        NodeEntity node,
        XrayConfig template,
        CancellationToken cancellationToken = default)
    {
        var desired = (template.Routing?.Rules ?? [])
            .Select(EnsureRuleTag)
            .GroupBy(rule => rule.RuleTag, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();
        var existing = await routingRules.GetByNodeIdAsync(node.Id, cancellationToken);
        var readonlyByRuleTag = existing
            .Where(rule => rule.ReadOnly && !string.IsNullOrWhiteSpace(rule.RuleTag))
            .ToDictionary(rule => rule.RuleTag!, StringComparer.Ordinal);
        var desiredRuleTags = desired.Select(rule => rule.RuleTag!).ToHashSet(StringComparer.Ordinal);

        foreach (var stale in readonlyByRuleTag.Values.Where(rule => !desiredRuleTags.Contains(rule.RuleTag!)).ToList())
        {
            await routingRules.DeleteAsync(stale.Id, cancellationToken);
        }

        var current = await routingRules.GetByNodeIdAsync(node.Id, cancellationToken);
        for (var index = 0; index < desired.Count; index++)
        {
            var config = desired[index];
            var existingReadonly = current.FirstOrDefault(rule => rule.ReadOnly
                && string.Equals(rule.RuleTag, config.RuleTag, StringComparison.Ordinal));
            if (!IsRuleTagAvailable(config, current, existingReadonly?.Id))
            {
                continue;
            }

            if (existingReadonly is null)
            {
                var created = await routingRules.AddAsync(
                    adminId,
                    node.Id,
                    new RoutingRuleEntity
                    {
                        Enabled = true,
                        ReadOnly = true,
                        Position = index * PositionStep,
                        Config = config
                    },
                    cancellationToken);
                current.Add(created);

                continue;
            }

            existingReadonly.Config = config;
            existingReadonly.Position = index * PositionStep;

            var updated = await routingRules.UpdateAsync(existingReadonly, cancellationToken);
            if (updated is null)
            {
                throw new NodeRoutingRuleNotFoundException($"Routing rule '{existingReadonly.Id}' was not found.");
            }
        }

        await ReindexAsync(node.Id, cancellationToken);
        await SyncRemoteRulesAsync(node, cancellationToken);
    }

    private static RoutingRule ParseRoutingRule(string config)
    {
        if (string.IsNullOrWhiteSpace(config))
        {
            throw new NodeRoutingRuleValidationException("Routing rule configuration is required.");
        }

        try
        {
            var rule = XrayJsonSerializer.Deserialize<RoutingRule>(config);
            if (rule is null)
            {
                throw new NodeRoutingRuleValidationException("Routing rule configuration is invalid.");
            }

            return EnsureRuleTag(rule);
        }
        catch (JsonException exception)
        {
            throw new NodeRoutingRuleValidationException($"Routing rule configuration is invalid. {exception.Message}");
        }
        catch (InvalidOperationException exception) when (exception is not NodeRoutingRuleValidationException)
        {
            throw new NodeRoutingRuleValidationException($"Routing rule configuration is invalid. {exception.Message}");
        }
    }

    private static RoutingRule EnsureRuleTag(RoutingRule rule)
    {
        if (string.IsNullOrWhiteSpace(rule.RuleTag))
        {
            rule.RuleTag = Guid.NewGuid().ToString();

            return rule;
        }

        rule.RuleTag = rule.RuleTag.Trim();

        return rule;
    }

    private static void ValidateRuleTag(
        RoutingRule config,
        IEnumerable<RoutingRuleEntity> existing,
        long? currentId)
    {
        if (!IsRuleTagAvailable(config, existing, currentId))
        {
            throw new NodeRoutingRuleValidationException($"Routing rule tag '{config.RuleTag}' already exists on this node.");
        }
    }

    private static bool IsRuleTagAvailable(
        RoutingRule config,
        IEnumerable<RoutingRuleEntity> existing,
        long? currentId)
    {
        return existing
            .Where(rule => rule.Id != currentId)
            .All(rule => !string.Equals(rule.RuleTag, config.RuleTag, StringComparison.Ordinal));
    }

    private static int GetNextManualPosition(IEnumerable<RoutingRuleEntity> current)
    {
        return current.Any()
            ? current.Max(rule => rule.Position) + PositionStep
            : 0;
    }

    private static void ValidateManualOrder(
        IReadOnlyList<long> ruleIds,
        IReadOnlyCollection<RoutingRuleEntity> manualRules)
    {
        var requested = ruleIds.ToHashSet();
        if (requested.Count != ruleIds.Count)
        {
            throw new NodeRoutingRuleValidationException("Routing rule order contains duplicate rule ids.");
        }

        var existing = manualRules.Select(rule => rule.Id).ToHashSet();
        if (!requested.SetEquals(existing))
        {
            throw new NodeRoutingRuleValidationException("Routing rule order must contain every manual routing rule id exactly once.");
        }
    }

    private static void ApplyManualPositions(
        IEnumerable<RoutingRuleEntity> current,
        IReadOnlyList<long> manualOrder)
    {
        var readonlyRules = current
            .Where(rule => rule.ReadOnly)
            .OrderBy(rule => rule.Position)
            .ThenBy(rule => rule.Id)
            .ToList();
        var manualById = current
            .Where(rule => !rule.ReadOnly)
            .ToDictionary(rule => rule.Id);
        var position = 0;

        foreach (var rule in readonlyRules)
        {
            rule.Position = position;
            position += PositionStep;
        }

        foreach (var ruleId in manualOrder)
        {
            manualById[ruleId].Position = position;
            position += PositionStep;
        }
    }

    private async Task ReindexAsync(long nodeId, CancellationToken cancellationToken)
    {
        var current = await routingRules.GetByNodeIdAsync(nodeId, cancellationToken);
        var manualOrder = current
            .Where(rule => !rule.ReadOnly)
            .OrderBy(rule => rule.Position)
            .ThenBy(rule => rule.Id)
            .Select(rule => rule.Id)
            .ToList();

        ApplyManualPositions(current, manualOrder);

        foreach (var rule in current)
        {
            var updated = await routingRules.UpdateAsync(rule, cancellationToken);
            if (updated is null)
            {
                throw new NodeRoutingRuleNotFoundException($"Routing rule '{rule.Id}' was not found.");
            }
        }
    }

    private async Task<NodeEntity> GetNodeAsync(long nodeId, CancellationToken cancellationToken)
    {
        var node = await nodes.GetByIdAsync(nodeId, cancellationToken);
        if (node is null)
        {
            throw new NodeRoutingRuleNotFoundException($"Node '{nodeId}' was not found.");
        }

        return node;
    }

    private async Task<RoutingRuleEntity> GetRoutingRuleAsync(
        long nodeId,
        long routingRuleId,
        CancellationToken cancellationToken)
    {
        var rule = await routingRules.GetByNodeAndIdAsync(nodeId, routingRuleId, cancellationToken);
        if (rule is null)
        {
            throw new NodeRoutingRuleNotFoundException($"Routing rule '{routingRuleId}' was not found.");
        }

        return rule;
    }

    private async Task SyncRemoteRulesAsync(NodeEntity node, CancellationToken cancellationToken)
    {
        if (!IsRemoteCoreRunning(node.Id))
        {
            return;
        }

        var enabledRules = (await routingRules.GetByNodeIdAsync(node.Id, cancellationToken))
            .Where(rule => rule.Enabled)
            .OrderBy(rule => rule.Position)
            .ThenBy(rule => rule.Id)
            .Select(rule => rule.Config)
            .ToList();

        await CreateRemoteNodeClient(node).SyncRoutingRulesAsync(
            new SyncRoutingRulesRequest { RoutingRules = enabledRules },
            cancellationToken);
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
