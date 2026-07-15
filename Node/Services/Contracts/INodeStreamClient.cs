using Node.Models;

namespace Node.Services;

/// <summary>
/// Opens authenticated gRPC streams to one remote node.
/// </summary>
public interface INodeStreamClient
{
    /// <summary>
    /// Opens and reads the remote node connection stream.
    /// </summary>
    IAsyncEnumerable<ConnectionEvent> ConnectStreamAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens and reads a remote node log stream.
    /// </summary>
    IAsyncEnumerable<RemoteLogStreamEvent> LogStreamAsync(
        int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens and reads the remote xray-core status stream.
    /// </summary>
    IAsyncEnumerable<CoreStatusResponse> CoreStatusStreamAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens and reads a remote xray-core installation status stream.
    /// </summary>
    IAsyncEnumerable<InstallCoreStatusResponse> InstallCoreStatusStreamAsync(
        string jobId,
        CancellationToken cancellationToken = default);
}
