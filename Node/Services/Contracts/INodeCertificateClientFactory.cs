using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates certificate clients for authenticated remote node endpoints.
/// </summary>
public interface INodeCertificateClientFactory
{
    /// <summary>
    /// Creates a certificate client for the provided remote node endpoint.
    /// </summary>
    INodeCertificateClient Create(NodeEndpoint endpoint);
}
