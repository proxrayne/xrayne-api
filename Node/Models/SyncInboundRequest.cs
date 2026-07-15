using Xray.Config.Models;

namespace Node.Models;

/// <summary>
/// Requests synchronization of a single inbound configuration with the running core.
/// </summary>
public sealed class SyncInboundRequest
{
    /// <summary>
    /// Gets the native xray-core inbound configuration.
    /// </summary>
    public required Inbound Inbound { get; init; }
}
