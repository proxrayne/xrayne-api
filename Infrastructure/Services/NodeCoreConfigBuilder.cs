using Contracts.Utilities;
using Data.Entities;
using RemoteNode.Models;
using Xray.Config.Models;

namespace Infrastructure.Services;

/// <summary>
/// Builds remote node xray-core runtime configurations.
/// </summary>
public sealed class NodeCoreConfigBuilder : INodeCoreConfigBuilder
{
    /// <inheritdoc />
    public StartCoreRequest Build(NodeEntity node)
    {
        var config = XrayJsonSerializer.Clone(node.ConfigTemplate, "Node config template cannot be empty.");
        StripManagedSections(config);

        var inbounds = node.Inbounds
             .Where(inbound => inbound.Enabled)
             .OrderBy(inbound => inbound.CreatedAt)
             .ThenBy(inbound => inbound.Id)
             .Select((inbound, index) => new InboundSyncItem
             {
                 Id = inbound.Id,
                 Position = index,
                 Inbound = inbound.Config
             })
             .ToList();
        var outbounds = node.Outbounds
             .Where(outbound => outbound.Enabled)
             .OrderBy(outbound => outbound.CreatedAt)
             .ThenBy(outbound => outbound.Id)
             .Select((outbound, index) => new OutboundSyncItem
             {
                 Id = outbound.Id,
                 Position = index,
                 Outbound = outbound.Config
             })
             .ToList();

        var routingRules = node.RoutingRules
            .Where(rule => rule.Enabled)
            .OrderBy(rule => rule.Position)
            .ThenBy(rule => rule.Id)
            .Select(rule => new RoutingRuleSyncItem
            {
                Id = rule.Id,
                Position = rule.Position,
                RoutingRule = rule.Config
            })
            .ToList();

        return new StartCoreRequest
        {
            ConfigTemplate = config,
            Inbounds = inbounds,
            Outbounds = outbounds,
            RoutingRules = routingRules
        };
    }

    private static void StripManagedSections(XrayConfig config)
    {
        config.Inbounds = null!;
        config.Outbounds = null!;
        if (config.Routing is not null)
        {
            config.Routing.Rules = null!;
        }
    }
}
