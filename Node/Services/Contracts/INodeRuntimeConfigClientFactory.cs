using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates runtime configuration clients for authenticated remote node endpoints.
/// </summary>
public interface INodeRuntimeConfigClientFactory
{
    /// <summary>
    /// Creates a runtime configuration client for the provided remote node endpoint.
    /// </summary>
    INodeRuntimeConfigClient Create(NodeEndpoint endpoint);
}
