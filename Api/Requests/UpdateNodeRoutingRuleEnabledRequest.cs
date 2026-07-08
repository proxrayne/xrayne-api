namespace Api.Requests;

/// <summary>
/// Requests an enabled state update for a node routing rule.
/// </summary>
public sealed class UpdateNodeRoutingRuleEnabledRequest
{
    /// <summary>
    /// Gets whether the routing rule should be enabled.
    /// </summary>
    public bool Enabled { get; init; }
}
