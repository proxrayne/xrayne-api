using System.Text.Json.Serialization;
using RemoteNode.Models.Json;

namespace RemoteNode.Models;

/// <summary>
/// Requests synchronization of ordered routing rules with the running core.
/// </summary>
[JsonConverter(typeof(SyncRoutingRulesRequestJsonConverter))]
public sealed class SyncRoutingRulesRequest
{
    /// <summary>
    /// Gets the enabled routing rules ordered by position.
    /// </summary>
    public List<RoutingRuleSyncItem> RoutingRules { get; init; } = [];
}
