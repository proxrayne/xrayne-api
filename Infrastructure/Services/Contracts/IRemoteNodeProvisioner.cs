using Data.Entities;
using Infrastructure.Dto;

namespace Infrastructure.Services;

/// <summary>
/// Provisions a remote node host.
/// </summary>
public interface IRemoteNodeProvisioner
{
    Task<RemoteNodeProvisionResult> ProvisionAsync(NodeEntity node, string apiKey, string jobId, CancellationToken cancellationToken);
}
