using Xray.Config.Models;
using System.Text.Json.Serialization;
using RemoteNode.Models.Json;

namespace RemoteNode.Models;

/// <summary>
/// Carries a managed routing rule configuration slice.
/// </summary>
[JsonConverter(typeof(RoutingRuleSyncItemJsonConverter))]
public sealed class RoutingRuleSyncItem
{
    /// <summary>
    /// Gets the panel-owned routing rule identifier.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the routing rule position in the effective xray-core configuration.
    /// </summary>
    public int Position { get; init; }

    /// <summary>
    /// Gets the routing rule configuration.
    /// </summary>
    public required RoutingRule RoutingRule { get; init; }
}
