using System.Runtime.CompilerServices;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;
using Node.Values;
using Proto = XRayne.ProtoTypes.RemoteNode.V1;

namespace Node.Services;

/// <summary>
/// Sends authenticated geo resource gRPC calls to a remote node.
/// </summary>
public sealed class NodeGeoResourceClient : NodeGrpcClientBase, INodeGeoResourceClient
{
    private readonly Proto.GeoResourceService.GeoResourceServiceClient client;

    /// <summary>
    /// Initializes a remote node geo resource client.
    /// </summary>
    public NodeGeoResourceClient(
        IOptions<NodeOptions> options,
        INodeGrpcChannelProvider channelProvider,
        NodeEndpoint endpoint)
        : base(options, channelProvider, endpoint)
    {
        client = new Proto.GeoResourceService.GeoResourceServiceClient(Channel);
    }

    /// <inheritdoc />
    public Task<List<GeoResourceDto>> GetGeoResourcesAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "ListGeoResources",
            callOptions => client.ListAsync(new Empty(), callOptions),
            response => response.Items.Select(NodeGrpcMapper.ToDomain).ToList(),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<GeoResourceContent> DownloadGeoResourceAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new GeoResourceContent(
            fileName,
            ReadGeoResourceDownloadChunksAsync(fileName, cancellationToken)));
    }

    private async IAsyncEnumerable<byte[]> ReadGeoResourceDownloadChunksAsync(
        string fileName,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var call = client.Download(
            new Proto.GeoResourceNameRequest { FileName = fileName },
            CreateStreamingCallOptions(cancellationToken));

        while (await MoveNextStreamMessageAsync("DownloadGeoResource", call.ResponseStream, cancellationToken))
        {
            yield return call.ResponseStream.Current.Content.ToByteArray();
        }
    }

    /// <inheritdoc />
    public async Task<GeoResourceDto> UploadGeoResourceAsync(
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteClientStreamingAsync(
            "Upload",
            callOptions => client.Upload(callOptions),
            (stream, token) => WriteGeoResourceUploadChunksAsync(fileName, content, stream, token),
            NodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    internal static async Task WriteGeoResourceUploadChunksAsync(
        string fileName,
        Stream content,
        IClientStreamWriter<Proto.UploadGeoResourceChunk> stream,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[GeoResourceTransferDefaults.ChunkSizeBytes];
        while (true)
        {
            var bytesRead = await content.ReadAsync(buffer, cancellationToken);
            if (bytesRead == 0)
            {
                return;
            }

            await stream.WriteAsync(new Proto.UploadGeoResourceChunk
            {
                FileName = fileName,
                Content = ByteString.CopyFrom(buffer, 0, bytesRead)
            });
        }
    }

    /// <inheritdoc />
    public Task<GeoResourceDto> RenameGeoResourceAsync(
        string fileName,
         string newFilename,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "Rename",
            callOptions => client.RenameAsync(
                new Proto.RenameGeoResourceRequest
                {
                    FileName = fileName,
                    NewFileName = newFilename
                },
                callOptions),
            NodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteGeoResourceAsync(string fileName, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "Delete",
            callOptions => client.DeleteAsync(new Proto.GeoResourceNameRequest { FileName = fileName }, callOptions),
            cancellationToken);
    }
}
