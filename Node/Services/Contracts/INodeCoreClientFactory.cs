using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates xray-core clients for authenticated remote node endpoints.
/// </summary>
public interface INodeCoreClientFactory
{
    /// <summary>
    /// Creates a core client for the provided remote node endpoint.
    /// </summary>
    INodeCoreClient Create(NodeEndpoint endpoint);
}
