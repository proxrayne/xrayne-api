using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;
using Proto = XRayne.ProtoTypes.RemoteNode.V1;

namespace Node.Services;

/// <summary>
/// Sends authenticated certificate gRPC calls to a remote node.
/// </summary>
public sealed class NodeCertificateClient : NodeGrpcClientBase, INodeCertificateClient
{
    private readonly Proto.CertificateService.CertificateServiceClient client;

    /// <summary>
    /// Initializes a remote node certificate client.
    /// </summary>
    public NodeCertificateClient(
        IOptions<NodeOptions> options,
        INodeGrpcChannelProvider channelProvider,
        NodeEndpoint endpoint)
        : base(options, channelProvider, endpoint)
    {
        client = new Proto.CertificateService.CertificateServiceClient(Channel);
    }

    /// <inheritdoc />
    public Task<List<CertificateDto>> GetCertificatesAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "ListCertificates",
            callOptions => client.ListAsync(new Empty(), callOptions),
            response => response.Items.Select(NodeGrpcMapper.ToDomain).ToList(),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<CertificateDto> IssueCertificateAsync(
        IssueCertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "IssueCertificate",
            callOptions => client.IssueAsync(new Proto.IssueCertificateRequest { Domain = request.Domain }, callOptions),
            NodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<CertificateDto> UploadCertificateAsync(
        UploadCertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "UploadCertificate",
            callOptions => client.UploadAsync(
                new Proto.UploadCertificateRequest
                {
                    Domain = request.Domain,
                    CertificateFile = request.CertificateFile,
                    PrivateKeyFile = request.PrivateKeyFile
                },
                callOptions),
            NodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<CertificateDto> RenewCertificateAsync(string domain, CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "RenewCertificate",
            callOptions => client.RenewAsync(new Proto.CertificateDomainRequest { Domain = domain }, callOptions),
            NodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteCertificateAsync(string domain, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "DeleteCertificate",
            callOptions => client.DeleteAsync(new Proto.CertificateDomainRequest { Domain = domain }, callOptions),
            cancellationToken);
    }
}
