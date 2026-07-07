using Contracts.Utilities;
using Data.Entities;
using Xray.Config.Models;

namespace Infrastructure.Services;

/// <summary>
/// Builds remote node xray-core runtime configurations.
/// </summary>
public sealed class NodeCoreConfigBuilder : INodeCoreConfigBuilder
{
    /// <inheritdoc />
    public XrayConfig Build(NodeEntity node)
    {
        var config = XrayJsonSerializer.Clone(node.ConfigTemplate, "Node config template cannot be empty.");

        var inbounds = node.Inbounds
             .Where(inbound => inbound.Enabled)
             .OrderBy(inbound => inbound.CreatedAt)
             .ThenBy(outbound => outbound.Id)
             .Select(inbound => inbound.Config)
             .ToList();
        var outbounds = node.Outbounds
             .Where(outbound => outbound.Enabled)
             .OrderBy(outbound => outbound.CreatedAt)
             .ThenBy(outbound => outbound.Id)
             .Select(outbound => outbound.Config)
             .ToList();

        var routingRules = node.RoutingRules
            .Where(rule => rule.Enabled)
            .OrderBy(rule => rule.Position)
            .ThenBy(rule => rule.Id)
            .Select(rule => rule.Config)
            .ToList();

        return config.Merge(new XrayConfig()
        {
            Inbounds = inbounds,
            Outbounds = outbounds,
            Routing = new RoutingConfig()
            {
                Rules = routingRules
            }
        });
    }
}
