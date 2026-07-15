using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;
using Proto = XRayne.ProtoTypes.RemoteNode.V1;

namespace Node.Services;

/// <summary>
/// Sends authenticated health and runtime gRPC calls to a remote node.
/// </summary>
public sealed class NodeHealthClient : NodeGrpcClientBase, INodeHealthClient
{
    private readonly Proto.HealthService.HealthServiceClient client;

    /// <summary>
    /// Initializes a remote node health client.
    /// </summary>
    public NodeHealthClient(
        IOptions<NodeOptions> options,
        INodeGrpcChannelProvider channelProvider,
        NodeEndpoint endpoint)
        : base(options, channelProvider, endpoint)
    {
        client = new Proto.HealthService.HealthServiceClient(Channel);
    }

    /// <inheritdoc />
    public Task<PingResponse> PingAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "Ping",
            callOptions => client.PingAsync(new Empty(), callOptions),
            NodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<SystemStatusResponse> GetSystemStatusAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "GetSystemStatus",
            callOptions => client.GetSystemStatusAsync(new Empty(), callOptions),
            NodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationAcceptedResponse> RestartRuntimeAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "RestartRuntime",
            callOptions => client.RestartRuntimeAsync(new Empty(), callOptions),
            NodeGrpcMapper.ToDomain,
            cancellationToken);
    }
}
