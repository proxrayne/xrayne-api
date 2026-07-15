using System.Runtime.CompilerServices;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Grpc;
using RemoteNode.Models;
using Proto = XRayne.ProtoTypes.RemoteNode.V1;

namespace RemoteNode.Services;

/// <summary>
/// Opens authenticated gRPC streams to one remote node.
/// </summary>
public sealed class RemoteNodeStreamClient(
    IOptions<RemoteNodeOptions> options,
    IRemoteNodeGrpcChannelProvider channelProvider,
    RemoteNodeEndpoint endpoint)
    : RemoteNodeGrpcClientBase(options, channelProvider, endpoint), IRemoteNodeStreamClient
{
    /// <inheritdoc />
    public async IAsyncEnumerable<ConnectionEvent> ConnectStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var call = HealthClient.Connect(new Empty(), CreateStreamingCallOptions(cancellationToken));
        while (await MoveNextStreamMessageAsync("Connect", call.ResponseStream, cancellationToken))
        {
            yield return RemoteNodeGrpcMapper.ToDomain(call.ResponseStream.Current);
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

        using var call = LogClient.StreamLogs(request, CreateStreamingCallOptions(cancellationToken));
        while (await MoveNextStreamMessageAsync("StreamLogs", call.ResponseStream, cancellationToken))
        {
            yield return RemoteNodeGrpcMapper.ToDomain(call.ResponseStream.Current);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<CoreStatusResponse> CoreStatusStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var call = CoreClient.StreamStatus(new Empty(), CreateStreamingCallOptions(cancellationToken));
        while (await MoveNextStreamMessageAsync("StreamCoreStatus", call.ResponseStream, cancellationToken))
        {
            yield return RemoteNodeGrpcMapper.ToDomain(call.ResponseStream.Current);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<InstallCoreStatusResponse> InstallCoreStatusStreamAsync(
        string jobId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var call = CoreClient.StreamInstallStatus(
            new Proto.GetInstallCoreStatusRequest { JobId = jobId },
            CreateStreamingCallOptions(cancellationToken));
        while (await MoveNextStreamMessageAsync("StreamInstallCoreStatus", call.ResponseStream, cancellationToken))
        {
            yield return RemoteNodeGrpcMapper.ToDomain(call.ResponseStream.Current);
        }
    }
}
