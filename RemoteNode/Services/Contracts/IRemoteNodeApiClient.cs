using RemoteNode.Models;

namespace RemoteNode.Services;

/// <summary>
/// Sends authenticated API requests to one remote node.
/// </summary>
public interface IRemoteNodeApiClient
{
    /// <summary>
    /// Gets current remote node telemetry.
    /// </summary>
    Task<NodePingResponse> PingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens and reads the remote node connection stream.
    /// </summary>
    IAsyncEnumerable<NodeConnectionEvent> ConnectStreamAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current remote node system status.
    /// </summary>
    Task<SystemStatusResponse> GetSystemStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current remote xray-core status.
    /// </summary>
    Task<CoreStatusResponse> GetCoreStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens and reads the remote xray-core status stream.
    /// </summary>
    IAsyncEnumerable<CoreStatusResponse> CoreStatusStreamAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules remote xray-core installation.
    /// </summary>
    Task<InstallCoreResponse> InstallCoreAsync(InstallCoreRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets remote xray-core installation status.
    /// </summary>
    Task<InstallCoreStatusResponse> GetInstallCoreStatusAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens and reads a remote xray-core installation status stream.
    /// </summary>
    IAsyncEnumerable<InstallCoreStatusResponse> InstallCoreStatusStreamAsync(
        string jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts remote xray-core.
    /// </summary>
    Task<OperationAcceptedResponse> StartCoreAsync(
        StartCoreRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops remote xray-core.
    /// </summary>
    Task<OperationAcceptedResponse> StopCoreAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Restarts remote xray-core.
    /// </summary>
    Task<OperationAcceptedResponse> RestartCoreAsync(
        StartCoreRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restarts the remote node service runtime.
    /// </summary>
    Task<OperationAcceptedResponse> RestartRuntimeAsync(CancellationToken cancellationToken = default);
}
