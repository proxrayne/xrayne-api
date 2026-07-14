using Xray.Config.Models;

namespace RemoteNode.Models;

/// <summary>
/// Requests synchronization of a single outbound configuration with the running core.
/// </summary>
public sealed class SyncOutboundRequest
{
    /// <summary>
    /// Gets the native xray-core outbound configuration.
    /// </summary>
    public required Outbound Outbound { get; init; }
}
