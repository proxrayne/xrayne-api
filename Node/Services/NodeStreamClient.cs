using System.Runtime.CompilerServices;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;
using Proto = XRayne.ProtoTypes.RemoteNode.V1;

namespace Node.Services;

/// <summary>
/// Opens authenticated gRPC streams to one remote node.
/// </summary>
public sealed class NodeStreamClient : NodeGrpcClientBase, INodeStreamClient
{
    private readonly Proto.HealthService.HealthServiceClient healthClient;
    private readonly Proto.CoreService.CoreServiceClient coreClient;
    private readonly Proto.LogService.LogServiceClient logClient;

    /// <summary>
    /// Initializes a remote node stream client.
    /// </summary>
    public NodeStreamClient(
        IOptions<NodeOptions> options,
        INodeGrpcChannelProvider channelProvider,
        NodeEndpoint endpoint)
        : base(options, channelProvider, endpoint)
    {
        healthClient = new Proto.HealthService.HealthServiceClient(Channel);
        coreClient = new Proto.CoreService.CoreServiceClient(Channel);
        logClient = new Proto.LogService.LogServiceClient(Channel);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ConnectionEvent> ConnectStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var call = healthClient.Connect(new Empty(), CreateStreamingCallOptions(cancellationToken));
        while (await MoveNextStreamMessageAsync("Connect", call.ResponseStream, cancellationToken))
        {
            yield return NodeGrpcMapper.ToDomain(call.ResponseStream.Current);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<RemoteLogStreamEvent> LogStreamAsync(
        int? limit = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new Proto.LogStreamRequest();
        if (limit is not null)
        {
            request.Limit = limit.Value;
        }

        using var call = logClient.StreamLogs(request, CreateStreamingCallOptions(cancellationToken));
        while (await MoveNextStreamMessageAsync("StreamLogs", call.ResponseStream, cancellationToken))
        {
            yield return NodeGrpcMapper.ToDomain(call.ResponseStream.Current);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<CoreStatusResponse> CoreStatusStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var call = coreClient.StreamStatus(new Empty(), CreateStreamingCallOptions(cancellationToken));
        while (await MoveNextStreamMessageAsync("StreamCoreStatus", call.ResponseStream, cancellationToken))
        {
            yield return NodeGrpcMapper.ToDomain(call.ResponseStream.Current);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<InstallCoreStatusResponse> InstallCoreStatusStreamAsync(
        string jobId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var call = coreClient.StreamInstallStatus(
            new Proto.GetInstallCoreStatusRequest { JobId = jobId },
            CreateStreamingCallOptions(cancellationToken));
        while (await MoveNextStreamMessageAsync("StreamInstallCoreStatus", call.ResponseStream, cancellationToken))
        {
            yield return NodeGrpcMapper.ToDomain(call.ResponseStream.Current);
        }
    }
}
