using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates health clients for authenticated remote node endpoints.
/// </summary>
public interface INodeHealthClientFactory
{
    /// <summary>
    /// Creates a health client for the provided remote node endpoint.
    /// </summary>
    INodeHealthClient Create(NodeEndpoint endpoint);
}
