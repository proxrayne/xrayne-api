using XRayne.Repositories.Entities;

namespace XRayne.Infrastructure.Services;

/// <summary>
/// Provisions a remote node host.
/// </summary>
public interface IRemoteNodeProvisioner
{
    Task<RemoteNodeProvisionResult> ProvisionAsync(NodeEntity node, string apiKey, string jobId, CancellationToken cancellationToken);
}
