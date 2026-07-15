using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;

namespace Node.Services;

/// <summary>
/// Creates remote node certificate clients.
/// </summary>
public sealed class NodeCertificateClientFactory(
    IOptions<NodeOptions> options,
    INodeGrpcChannelProvider channelProvider) : INodeCertificateClientFactory
{
    /// <inheritdoc />
    public INodeCertificateClient Create(NodeEndpoint endpoint)
    {
        return new NodeCertificateClient(options, channelProvider, endpoint);
    }
}
