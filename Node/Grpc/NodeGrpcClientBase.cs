using System.Net;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Exceptions;
using Node.Models;

namespace Node.Grpc;

/// <summary>
/// Provides shared gRPC transport behavior for remote node clients.
/// </summary>
public abstract class NodeGrpcClientBase
{
    private const string ApiKeyHeaderName = "X-Node-Api-Key";

    /// <summary>
    /// Initializes a remote node gRPC client transport.
    /// </summary>
    protected NodeGrpcClientBase(
        IOptions<NodeOptions> options,
        INodeGrpcChannelProvider channelProvider,
        NodeEndpoint endpoint)
    {
        Options = options.Value;
        Endpoint = endpoint;
        Channel = channelProvider.CreateChannel(endpoint);
    }

    /// <summary>
    /// Gets the remote node endpoint.
    /// </summary>
    protected NodeEndpoint Endpoint { get; }

    /// <summary>
    /// Gets the remote node client options.
    /// </summary>
    protected NodeOptions Options { get; }

    /// <summary>
    /// Gets the shared gRPC channel for the remote node endpoint.
    /// </summary>
    protected GrpcChannel Channel { get; }

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
            throw new NodeTimeoutException(Endpoint.NodeId, operation, exception);
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

    private NodeException MapRpcException(string operation, RpcException exception)
    {
        return exception.StatusCode switch
        {
            StatusCode.Unauthenticated => new NodeUnauthorizedException(
                Endpoint.NodeId,
                operation,
                HttpStatusCode.Unauthorized),
            StatusCode.PermissionDenied => new NodeUnauthorizedException(
                Endpoint.NodeId,
                operation,
                HttpStatusCode.Forbidden),
            StatusCode.Unavailable => new NodeUnavailableException(Endpoint.NodeId, operation, exception),
            StatusCode.DeadlineExceeded => new NodeTimeoutException(Endpoint.NodeId, operation, exception),
            StatusCode.Cancelled => new NodeUnavailableException(Endpoint.NodeId, operation, exception),
            StatusCode.NotFound => new NodeHttpException(
                Endpoint.NodeId,
                operation,
                HttpStatusCode.NotFound,
                exception.Status.Detail),
            StatusCode.AlreadyExists => new NodeHttpException(
                Endpoint.NodeId,
                operation,
                HttpStatusCode.Conflict,
                exception.Status.Detail),
            StatusCode.InvalidArgument => new NodeHttpException(
                Endpoint.NodeId,
                operation,
                HttpStatusCode.BadRequest,
                exception.Status.Detail),
            StatusCode.FailedPrecondition => new NodeHttpException(
                Endpoint.NodeId,
                operation,
                HttpStatusCode.BadRequest,
                exception.Status.Detail),
            _ => new NodeProtocolException(
                Endpoint.NodeId,
                operation,
                $"gRPC status '{exception.StatusCode}': {exception.Status.Detail}",
                exception)
        };
    }

}
