using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;
using Proto = XRayne.ProtoTypes.RemoteNode.V1;

namespace Node.Services;

/// <summary>
/// Sends authenticated log gRPC calls to a remote node.
/// </summary>
public sealed class NodeLogClient : NodeGrpcClientBase, INodeLogClient
{
    private readonly Proto.LogService.LogServiceClient client;

    /// <summary>
    /// Initializes a remote node log client.
    /// </summary>
    public NodeLogClient(
        IOptions<NodeOptions> options,
        INodeGrpcChannelProvider channelProvider,
        NodeEndpoint endpoint)
        : base(options, channelProvider, endpoint)
    {
        client = new Proto.LogService.LogServiceClient(Channel);
    }

    /// <inheritdoc />
    public Task<RemoteLogSnapshotResponse> GetLogsAsync(
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "GetLogs",
            callOptions =>
            {
                var request = new Proto.LogStreamRequest();
                if (limit is not null)
                {
                    request.Limit = limit.Value;
                }

                return client.GetLogsAsync(request, callOptions);
            },
            response => new RemoteLogSnapshotResponse(response.Limit, [.. response.Entries.Select(NodeGrpcMapper.ToDomain)]),
            cancellationToken);
    }
}
