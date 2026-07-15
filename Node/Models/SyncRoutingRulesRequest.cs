using Xray.Config.Models;

namespace Node.Models;

/// <summary>
/// Requests synchronization of ordered routing rules with the running core.
/// </summary>
public sealed class SyncRoutingRulesRequest
{
    /// <summary>
    /// Gets the enabled native routing rules ordered by position.
    /// </summary>
    public List<RoutingRule> RoutingRules { get; init; } = [];
}
