using Xray.Config.Models;

namespace RemoteNode.Models;

/// <summary>
/// Carries a managed outbound configuration slice.
/// </summary>
public class OutboundSyncItem
{
    /// <summary>
    /// Gets the panel-owned outbound identifier.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the outbound position in the effective xray-core configuration.
    /// </summary>
    public int Position { get; init; }

    /// <summary>
    /// Gets the outbound configuration.
    /// </summary>
    public required Outbound Outbound { get; init; }
}
