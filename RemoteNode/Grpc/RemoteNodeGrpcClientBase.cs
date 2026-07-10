using System.Net;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Exceptions;
using RemoteNode.Models;
using Proto = XRayne.ProtoTypes.RemoteNode.V1;

namespace RemoteNode.Grpc;

/// <summary>
/// Provides shared gRPC transport behavior for remote node clients.
/// </summary>
public abstract class RemoteNodeGrpcClientBase
{
    private const string ApiKeyHeaderName = "X-Node-Api-Key";

    /// <summary>
    /// Initializes a remote node gRPC client transport.
    /// </summary>
    protected RemoteNodeGrpcClientBase(
        IOptions<RemoteNodeOptions> options,
        IRemoteNodeGrpcChannelProvider channelProvider,
        RemoteNodeEndpoint endpoint)
    {
        Options = options.Value;
        Endpoint = endpoint;
        Client = channelProvider.CreateClient(endpoint);
    }

    /// <summary>
    /// Gets the remote node endpoint.
    /// </summary>
    protected RemoteNodeEndpoint Endpoint { get; }

    /// <summary>
    /// Gets the remote node client options.
    /// </summary>
    protected RemoteNodeOptions Options { get; }

    /// <summary>
    /// Gets the generated gRPC client.
    /// </summary>
    protected Proto.RemoteNodeService.RemoteNodeServiceClient Client { get; }

    /// <summary>
    /// Executes a unary gRPC call and maps transport failures.
    /// </summary>
    protected async Task<TResult> ExecuteUnaryAsync<TResponse, TResult>(
        string operation,
        Func<CallOptions, AsyncUnaryCall<TResponse>> callFactory,
        Func<TResponse, TResult> map,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await callFactory(CreateUnaryCallOptions(cancellationToken)).ResponseAsync;
            return map(response);
        }
        catch (RpcException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw MapRpcException(operation, exception);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new RemoteNodeTimeoutException(Endpoint.NodeId, operation, exception);
        }
    }

    /// <summary>
    /// Executes a unary gRPC call that returns no domain payload.
    /// </summary>
    protected async Task ExecuteEmptyUnaryAsync(
        string operation,
        Func<CallOptions, AsyncUnaryCall<Empty>> callFactory,
        CancellationToken cancellationToken)
    {
        await ExecuteUnaryAsync(operation, callFactory, static _ => true, cancellationToken);
    }

    /// <summary>
    /// Advances a server stream and maps gRPC failures.
    /// </summary>
    protected async Task<bool> MoveNextStreamMessageAsync<T>(
        string operation,
        IAsyncStreamReader<T> reader,
        CancellationToken cancellationToken)
    {
        try
        {
            return await reader.MoveNext(cancellationToken);
        }
        catch (RpcException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw MapRpcException(operation, exception);
        }
    }

    /// <summary>
    /// Creates call options for long-lived streaming calls.
    /// </summary>
    protected CallOptions CreateStreamingCallOptions(CancellationToken cancellationToken)
    {
        return new CallOptions(headers: CreateHeaders(), cancellationToken: cancellationToken);
    }

    private CallOptions CreateUnaryCallOptions(CancellationToken cancellationToken)
    {
        var timeoutSeconds = Math.Max(1, Options.PingTimeoutSeconds);

        return new CallOptions(
            headers: CreateHeaders(),
            deadline: DateTime.UtcNow.AddSeconds(timeoutSeconds),
            cancellationToken: cancellationToken);
    }

    private Metadata CreateHeaders()
    {
        return new Metadata
        {
            { ApiKeyHeaderName, Endpoint.ApiKey }
        };
    }

    private RemoteNodeException MapRpcException(string operation, RpcException exception)
    {
        return exception.StatusCode switch
        {
            StatusCode.Unauthenticated => new RemoteNodeUnauthorizedException(
                Endpoint.NodeId,
                operation,
                HttpStatusCode.Unauthorized),
            StatusCode.PermissionDenied => new RemoteNodeUnauthorizedException(
                Endpoint.NodeId,
                operation,
                HttpStatusCode.Forbidden),
            StatusCode.Unavailable => new RemoteNodeUnavailableException(Endpoint.NodeId, operation, exception),
            StatusCode.DeadlineExceeded => new RemoteNodeTimeoutException(Endpoint.NodeId, operation, exception),
            StatusCode.Cancelled => new RemoteNodeUnavailableException(Endpoint.NodeId, operation, exception),
            StatusCode.NotFound => new RemoteNodeHttpException(
                Endpoint.NodeId,
                operation,
                HttpStatusCode.NotFound,
                exception.Status.Detail),
            StatusCode.AlreadyExists => new RemoteNodeHttpException(
                Endpoint.NodeId,
                operation,
                HttpStatusCode.Conflict,
                exception.Status.Detail),
            StatusCode.InvalidArgument => new RemoteNodeHttpException(
                Endpoint.NodeId,
                operation,
                HttpStatusCode.BadRequest,
                exception.Status.Detail),
            StatusCode.FailedPrecondition => new RemoteNodeHttpException(
                Endpoint.NodeId,
                operation,
                HttpStatusCode.BadRequest,
                exception.Status.Detail),
            _ => new RemoteNodeProtocolException(
                Endpoint.NodeId,
                operation,
                $"gRPC status '{exception.StatusCode}': {exception.Status.Detail}",
                exception)
        };
    }

}
