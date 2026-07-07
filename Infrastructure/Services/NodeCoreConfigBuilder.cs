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
        var template = XrayJsonSerializer.Clone(node.ConfigTemplate, "Node config template cannot be empty.");
        var managedConfig = new XrayConfig
        {
            Inbounds = node.Inbounds
                .Where(inbound => inbound.Enabled)
                .OrderBy(inbound => inbound.Id)
                .Select(inbound => inbound.Config)
                .ToList(),
            Outbounds = node.Outbounds
                .Where(outbound => outbound.Enabled)
                .OrderBy(outbound => outbound.CreatedAt)
                .ThenBy(outbound => outbound.Id)
                .Select(outbound => outbound.Config)
                .ToList(),
            Routing = new RoutingConfig
            {
                Rules = node.RoutingRules
                    .Where(rule => rule.Enabled)
                    .OrderBy(rule => rule.Position)
                    .ThenBy(rule => rule.Id)
                    .Select(rule => rule.Config)
                    .ToList()
            }
        };

        return template.Merge(managedConfig, new XrayMergeOptions
        {
            CollectionMode = XrayMergeCollectionMode.Replace,
            IgnoreDefaultValueTypes = true,
            IgnoreEmptyCollections = true
        });
    }
}
