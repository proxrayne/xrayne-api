using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates log clients for authenticated remote node endpoints.
/// </summary>
public interface INodeLogClientFactory
{
    /// <summary>
    /// Creates a log client for the provided remote node endpoint.
    /// </summary>
    INodeLogClient Create(NodeEndpoint endpoint);
}
