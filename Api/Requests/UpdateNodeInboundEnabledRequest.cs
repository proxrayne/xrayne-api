namespace Api.Requests;

/// <summary>
/// Requests an enabled-state change for a node inbound.
/// </summary>
public sealed class UpdateNodeInboundEnabledRequest
{
    /// <summary>
    /// Gets whether the inbound should be enabled.
    /// </summary>
    public bool Enabled { get; init; }
}
