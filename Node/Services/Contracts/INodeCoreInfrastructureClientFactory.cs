using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates xray-core utility clients for authenticated remote node endpoints.
/// </summary>
public interface INodeCoreInfrastructureClientFactory
{
    /// <summary>
    /// Creates an xray-core utility client for the provided remote node endpoint.
    /// </summary>
    INodeCoreInfrastructureClient Create(NodeEndpoint endpoint);
}
