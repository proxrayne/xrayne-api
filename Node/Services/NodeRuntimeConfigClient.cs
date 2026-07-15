using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;
using Proto = XRayne.ProtoTypes.RemoteNode.V1;

namespace Node.Services;

/// <summary>
/// Sends authenticated runtime configuration gRPC calls to a remote node.
/// </summary>
public sealed class NodeRuntimeConfigClient : NodeGrpcClientBase, INodeRuntimeConfigClient
{
    private readonly Proto.RuntimeConfigService.RuntimeConfigServiceClient client;

    /// <summary>
    /// Initializes a remote node runtime configuration client.
    /// </summary>
    public NodeRuntimeConfigClient(
        IOptions<NodeOptions> options,
        INodeGrpcChannelProvider channelProvider,
        NodeEndpoint endpoint)
        : base(options, channelProvider, endpoint)
    {
        client = new Proto.RuntimeConfigService.RuntimeConfigServiceClient(Channel);
    }

    /// <inheritdoc />
    public Task AddInboundAsync(SyncInboundRequest request, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "AddInbound",
            callOptions => client.AddInboundAsync(NodeGrpcMapper.ToProto(request), callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateInboundAsync(
        string id,
        SyncInboundRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "UpdateInbound",
            callOptions => client.UpdateInboundAsync(
                new Proto.UpdateInboundRequest
                {
                    Id = id,
                    Request = NodeGrpcMapper.ToProto(request)
                },
                callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteInboundAsync(string id, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "DeleteInbound",
            callOptions => client.DeleteInboundAsync(new Proto.DeleteManagedSliceRequest { Id = id }, callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task AddOutboundAsync(SyncOutboundRequest request, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "AddOutbound",
            callOptions => client.AddOutboundAsync(NodeGrpcMapper.ToProto(request), callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateOutboundAsync(
        string id,
        SyncOutboundRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "UpdateOutbound",
            callOptions => client.UpdateOutboundAsync(
                new Proto.UpdateOutboundRequest
                {
                    Id = id,
                    Request = NodeGrpcMapper.ToProto(request)
                },
                callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteOutboundAsync(string id, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "DeleteOutbound",
            callOptions => client.DeleteOutboundAsync(new Proto.DeleteManagedSliceRequest { Id = id }, callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task SyncRoutingRulesAsync(
        SyncRoutingRulesRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "SyncRoutingRules",
            callOptions => client.SyncRoutingRulesAsync(NodeGrpcMapper.ToProto(request), callOptions),
            cancellationToken);
    }
}
