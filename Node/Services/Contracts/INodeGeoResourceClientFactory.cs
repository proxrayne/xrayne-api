using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates geo resource clients for authenticated remote node endpoints.
/// </summary>
public interface INodeGeoResourceClientFactory
{
    /// <summary>
    /// Creates a geo resource client for the provided remote node endpoint.
    /// </summary>
    INodeGeoResourceClient Create(NodeEndpoint endpoint);
}
