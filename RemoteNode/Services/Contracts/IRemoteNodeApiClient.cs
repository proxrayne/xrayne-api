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
    /// Gets recent remote node log entries.
    /// </summary>
    Task<RemoteLogSnapshotResponse> GetLogsAsync(
        int? limit = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens and reads a remote node log stream.
    /// </summary>
    IAsyncEnumerable<RemoteLogStreamEvent> LogStreamAsync(
        int? limit = null,
        CancellationToken cancellationToken = default);

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

    /// <summary>
    /// Adds an inbound to the remote node runtime when xray-core is started.
    /// </summary>
    Task AddInboundAsync(SyncInboundRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces an inbound in the remote node runtime when xray-core is started.
    /// </summary>
    Task UpdateInboundAsync(string tag, SyncInboundRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an inbound from the remote node runtime when xray-core is started.
    /// </summary>
    Task DeleteInboundAsync(string tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an outbound to the remote node runtime when xray-core is started.
    /// </summary>
    Task AddOutboundAsync(SyncOutboundRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces an outbound in the remote node runtime when xray-core is started.
    /// </summary>
    Task UpdateOutboundAsync(string tag, SyncOutboundRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an outbound from the remote node runtime when xray-core is started.
    /// </summary>
    Task DeleteOutboundAsync(string tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces routing rules in the remote node runtime when xray-core is started.
    /// </summary>
    Task SyncRoutingRulesAsync(SyncRoutingRulesRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets certificates available on the remote node.
    /// </summary>
    Task<List<CertificateDto>> GetCertificatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Issues a Let's Encrypt certificate on the remote node.
    /// </summary>
    Task<CertificateDto> IssueCertificateAsync(
        IssueCertificateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a certificate and private key to the remote node.
    /// </summary>
    Task<CertificateDto> UploadCertificateAsync(
        UploadCertificateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renews an existing remote node certificate.
    /// </summary>
    Task<CertificateDto> RenewCertificateAsync(string domain, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a remote node certificate directory.
    /// </summary>
    Task DeleteCertificateAsync(string domain, CancellationToken cancellationToken = default);
}
