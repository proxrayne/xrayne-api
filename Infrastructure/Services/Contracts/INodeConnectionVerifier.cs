using Repositories.Entities;

namespace Infrastructure.Services;

/// <summary>
/// Verifies that a remote node API is reachable and authenticated.
/// </summary>
public interface INodeConnectionVerifier
{
    /// <summary>
    /// Sends an authenticated ping request to the node API.
    /// </summary>
    Task<NodeConnectionVerificationResult> VerifyAsync(
        NodeEntity node,
        string apiKey,
        CancellationToken cancellationToken);
}
