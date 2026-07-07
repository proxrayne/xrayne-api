namespace Api.Requests;

/// <summary>
/// Requests an enabled-state change for a node outbound.
/// </summary>
public sealed class UpdateNodeOutboundEnabledRequest
{
    /// <summary>
    /// Gets whether the outbound should be enabled.
    /// </summary>
    public bool Enabled { get; init; }
}
