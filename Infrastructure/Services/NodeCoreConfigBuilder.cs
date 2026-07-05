using Repositories.Entities;
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
        var template = XrayConfig.FromJson(node.ConfigTemplate.ToJson());
        var managedConfig = new XrayConfig
        {
            Inbounds = node.Inbounds
                .Where(inbound => inbound.Enabled)
                .OrderBy(inbound => inbound.Id)
                .Select(inbound => inbound.Config)
                .ToList(),
            Outbounds = node.Outbounds
                .Where(outbound => outbound.Enabled)
                .OrderBy(outbound => outbound.Position)
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

        var merged = template.Merge(managedConfig, new XrayMergeOptions
        {
            CollectionMode = XrayMergeCollectionMode.Replace,
            IgnoreDefaultValueTypes = true,
            IgnoreEmptyCollections = true
        });

        merged.Inbounds = managedConfig.Inbounds;
        merged.Outbounds = managedConfig.Outbounds;
        merged.Routing ??= new RoutingConfig();
        merged.Routing.Rules = managedConfig.Routing.Rules;

        return merged;
    }
}
