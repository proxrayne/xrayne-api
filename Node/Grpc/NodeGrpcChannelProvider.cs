using System.Collections.Concurrent;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Models;

namespace Node.Grpc;

/// <summary>
/// Reuses gRPC channels per remote node address and port.
/// </summary>
public sealed class NodeGrpcChannelProvider(IOptions<NodeOptions> options) :
    INodeGrpcChannelProvider,
    IDisposable
{
    private readonly ConcurrentDictionary<string, GrpcChannel> channels = new();

    /// <inheritdoc />
    public GrpcChannel CreateChannel(NodeEndpoint endpoint)
    {
        return channels.GetOrAdd(BuildKey(endpoint), _ => CreateGrpcChannel(endpoint));
    }

    /// <inheritdoc />
    public void Invalidate(NodeEndpoint endpoint)
    {
        if (channels.TryRemove(BuildKey(endpoint), out var channel))
        {
            channel.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var channel in channels.Values)
        {
            channel.Dispose();
        }

        channels.Clear();
    }

    private GrpcChannel CreateGrpcChannel(NodeEndpoint endpoint)
    {
        var configured = options.Value;
        var handler = new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true,
            KeepAlivePingDelay = TimeSpan.FromSeconds(Math.Max(10, configured.KeepAlivePingDelaySeconds)),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(Math.Max(5, configured.KeepAlivePingTimeoutSeconds)),
            KeepAlivePingPolicy = HttpKeepAlivePingPolicy.WithActiveRequests
        };

        return GrpcChannel.ForAddress(BuildAddress(endpoint), new GrpcChannelOptions
        {
            HttpHandler = handler,
            MaxReceiveMessageSize = configured.MaxMessageSizeBytes,
            MaxSendMessageSize = configured.MaxMessageSizeBytes
        });
    }

    private static string BuildKey(NodeEndpoint endpoint)
    {
        return $"{endpoint.Address}:{endpoint.ApiPort}";
    }

    private static Uri BuildAddress(NodeEndpoint endpoint)
    {
        return new UriBuilder("https", endpoint.Address, endpoint.ApiPort).Uri;
    }
}
