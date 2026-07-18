using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;
using Xray.Config.Models;
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
    public Task AddInboundAsync(Inbound inbound, CancellationToken ct = default)
    {
        return ExecuteEmptyUnaryAsync(
            "AddInbound",
            opt => client.AddInboundAsync(new Proto.SyncInboundRequest()
            {
                InboundJson = NodeGrpcMapper.SerializeXray(inbound)
            }, opt),
            ct);
    }

    /// <inheritdoc />
    public Task UpdateInboundAsync(
        string id,
        Inbound inbound,
        CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "UpdateInbound",
            callOptions => client.UpdateInboundAsync(
                new Proto.UpdateInboundRequest
                {
                    Id = id,
                    InboundJson = NodeGrpcMapper.SerializeXray(inbound)
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
    public Task AddOutboundAsync(Outbound outbound, CancellationToken ct = default)
    {
        return ExecuteEmptyUnaryAsync(
            "AddOutbound",
            opt => client.AddOutboundAsync(new Proto.SyncOutboundRequest()
            {
                OutboundJson = NodeGrpcMapper.SerializeXray(outbound)
            }, opt),
            ct);
    }

    /// <inheritdoc />
    public Task UpdateOutboundAsync(
        string id,
        Outbound outbound,
        CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "UpdateOutbound",
            callOptions => client.UpdateOutboundAsync(
                new Proto.UpdateOutboundRequest
                {
                    Id = id,
                    OutboundJson = NodeGrpcMapper.SerializeXray(outbound)
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
        IReadOnlyCollection<RoutingRule> routingRules,
        CancellationToken ct = default)
    {
        var request = new Proto.SyncRoutingRulesRequest();

        request.RoutingRuleJson.AddRange(routingRules.Select(NodeGrpcMapper.SerializeXray));

        return ExecuteEmptyUnaryAsync(
            "SyncRoutingRules",
            opt => client.SyncRoutingRulesAsync(request, opt),
            ct);
    }
}
