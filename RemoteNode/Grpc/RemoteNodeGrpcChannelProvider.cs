using System.Collections.Concurrent;
using System.Net.Http;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Models;

namespace RemoteNode.Grpc;

/// <summary>
/// Reuses gRPC channels per remote node address and port.
/// </summary>
public sealed class RemoteNodeGrpcChannelProvider(IOptions<RemoteNodeOptions> options) :
    IRemoteNodeGrpcChannelProvider,
    IDisposable
{
    private readonly ConcurrentDictionary<string, GrpcChannel> channels = new();

    /// <inheritdoc />
    public GrpcChannel CreateChannel(RemoteNodeEndpoint endpoint)
    {
        return channels.GetOrAdd(BuildKey(endpoint), _ => CreateGrpcChannel(endpoint));
    }

    /// <inheritdoc />
    public void Invalidate(RemoteNodeEndpoint endpoint)
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

    private GrpcChannel CreateGrpcChannel(RemoteNodeEndpoint endpoint)
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
            HttpHandler = handler
        });
    }

    private static string BuildKey(RemoteNodeEndpoint endpoint)
    {
        return $"{endpoint.Address}:{endpoint.ApiPort}";
    }

    private static Uri BuildAddress(RemoteNodeEndpoint endpoint)
    {
        return new UriBuilder("https", endpoint.Address, endpoint.ApiPort).Uri;
    }
}
