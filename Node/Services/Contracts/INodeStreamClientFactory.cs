using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates gRPC stream clients for remote nodes.
/// </summary>
public interface INodeStreamClientFactory
{
    /// <summary>
    /// Creates a stream client for one remote node endpoint.
    /// </summary>
    INodeStreamClient Create(NodeEndpoint endpoint);
}
