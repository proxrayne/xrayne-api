using Node.Models;

namespace Node.Services;

/// <summary>
/// Sends authenticated health and runtime requests to one remote node.
/// </summary>
public interface INodeHealthClient
{
    /// <summary>
    /// Gets current remote node telemetry.
    /// </summary>
    Task<PingResponse> PingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current remote node system status.
    /// </summary>
    Task<SystemStatusResponse> GetSystemStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Restarts the remote node service runtime.
    /// </summary>
    Task<OperationAcceptedResponse> RestartRuntimeAsync(CancellationToken cancellationToken = default);
}
