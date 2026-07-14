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

        config.Inbounds = node.Inbounds
             .Where(inbound => inbound.Enabled)
             .OrderBy(inbound => inbound.CreatedAt)
             .ThenBy(inbound => inbound.Id)
             .Select(inbound => inbound.Config)
             .ToList();
        config.Outbounds = node.Outbounds
             .Where(outbound => outbound.Enabled)
             .OrderBy(outbound => outbound.CreatedAt)
             .ThenBy(outbound => outbound.Id)
             .Select(outbound => outbound.Config)
             .ToList();

        config.Routing ??= new RoutingConfig();
        config.Routing.Rules = node.RoutingRules
            .Where(rule => rule.Enabled)
            .OrderBy(rule => rule.Position)
            .ThenBy(rule => rule.Id)
            .Select(rule => rule.Config)
            .ToList();

        return new StartCoreRequest
        {
            Config = config
        };
    }
}
