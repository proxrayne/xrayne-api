using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;
using Proto = XRayne.ProtoTypes.RemoteNode.V1;

namespace Node.Services;

/// <summary>
/// Sends authenticated xray-core utility gRPC calls to a remote node.
/// </summary>
public sealed class NodeCoreInfrastructureClient : NodeGrpcClientBase, INodeCoreInfrastructureClient
{
    private readonly Proto.CoreInfrastructureService.CoreInfrastructureServiceClient client;

    /// <summary>
    /// Initializes a remote node xray-core utility client.
    /// </summary>
    public NodeCoreInfrastructureClient(
        IOptions<NodeOptions> options,
        INodeGrpcChannelProvider channelProvider,
        NodeEndpoint endpoint)
        : base(options, channelProvider, endpoint)
    {
        client = new Proto.CoreInfrastructureService.CoreInfrastructureServiceClient(Channel);
    }

    /// <inheritdoc />
    public async Task<X25519KeysResponse> GetX25519KeysAsync(string? privateKey, CancellationToken ct = default)
    {
        var result = await ExecuteUnaryAsync(
            "GetX25519Keys",
            opt => client.GetX25519KeysAsync(new Proto.GetX25519KeysRequest() { PrivateKey = privateKey }, opt),
            ct);

        return new X25519KeysResponse(result.PrivateKey, result.Password, result.Hash);
    }

    /// <inheritdoc />
    public async Task<string> GetUuidAsync(string input, CancellationToken ct = default)
    {
        var result = await ExecuteUnaryAsync(
            "GetUuid",
            opt => client.GetUuidAsync(new Proto.GetUuidRequest() { Input = input }, opt),
            ct);

        return result.Uuid;
    }

    /// <inheritdoc />
    public async Task<string> GetUuidAsync(CancellationToken ct = default)
    {
        var result = await ExecuteUnaryAsync(
            "GetUuid",
            opt => client.GetUuidAsync(new Proto.GetUuidRequest(), opt),
            ct);

        return result.Uuid;
    }

    /// <inheritdoc />
    public async Task<Mldsa65Response> GetMLDSA65Async(
        string input,
        CancellationToken ct = default)
    {
        var result = await ExecuteUnaryAsync(
            "GetMLDSA65",
            opt => client.GetMLDSA65Async(new Proto.GetMLDSA65Request() { Input = input }, opt),
            ct);

        return new Mldsa65Response(result.Seed, result.Verify);
    }

    /// <inheritdoc />
    public async Task<Mlkem768Response> GetMLKEM768Async(
        string input,
        CancellationToken ct = default)
    {
        var result = await ExecuteUnaryAsync(
            "GetMLKEM768",
            opt => client.GeMLKEM768Async(new Proto.GetMLKEM768Request() { Input = input }, opt),
            NodeGrpcMapper.ToDomain,
            ct);

        return new Mlkem768Response(result.Seed, result.Client, result.Hash);
    }

    /// <inheritdoc />
    public async Task<VlessAuthResponse> GetVlessAuthAsync(CancellationToken ct = default)
    {
        var result = await ExecuteUnaryAsync(
            "GetVlessAuth",
            opt => client.GeVlessAuthAsync(new Empty(), opt),
            ct);

        return new VlessAuthResponse(
            new VlessAuthPair(result.X25519.Decryption, result.X25519.Encryption),
            new VlessAuthPair(result.MLKEM768.Decryption, result.MLKEM768.Encryption)
        );
    }

    /// <inheritdoc />
    public async Task<GetCertsResponse> GetCertsAsync(
        GetCertsRequest options,
        CancellationToken ct = default)
    {
        var request = new Proto.GetCertsRequest
        {
            IsCa = options.IsCA
        };

        request.Domains.AddRange(options.Domains);

        if (!string.IsNullOrWhiteSpace(options.CommonName))
        {
            request.CommonName = options.CommonName;
        }

        if (!string.IsNullOrWhiteSpace(options.Organization))
        {
            request.Organization = options.Organization;
        }

        if (!string.IsNullOrWhiteSpace(options.Expire))
        {
            request.Expire = options.Expire;
        }

        var result = await ExecuteUnaryAsync(
            "GetCerts",
            opt => client.GetCertsAsync(request, opt),
            ct);

        return new GetCertsResponse([.. result.Certificate], [.. result.Key]);
    }
}
