using Xray.Config.Models;

namespace RemoteNode.Models;

/// <summary>
/// Requests xray-core start with a full runtime configuration snapshot.
/// </summary>
public sealed class StartCoreRequest
{
    /// <summary>
    /// Gets the full xray-core configuration.
    /// </summary>
    public required XrayConfig Config { get; init; }
}
