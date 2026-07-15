using Node.Models;

namespace Node.Services;

/// <summary>
/// Sends authenticated xray-core lifecycle requests to one remote node.
/// </summary>
public interface INodeCoreClient
{
    /// <summary>
    /// Gets current remote xray-core status.
    /// </summary>
    Task<CoreStatusResponse> GetCoreStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules remote xray-core installation.
    /// </summary>
    Task<InstallCoreResponse> InstallCoreAsync(InstallCoreRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets remote xray-core installation status.
    /// </summary>
    Task<InstallCoreStatusResponse> GetInstallCoreStatusAsync(string jobId, CancellationToken cancellationToken = default);

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
    /// Updates the remote node base xray-core configuration template.
    /// </summary>
    Task UpdateCoreConfigTemplateAsync(
        UpdateCoreConfigTemplateRequest request,
        CancellationToken cancellationToken = default);
}
