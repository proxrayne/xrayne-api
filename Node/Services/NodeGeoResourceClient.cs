using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;
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
        return ExecuteUnaryAsync(
            "DownloadGeoResource",
            callOptions => client.DownloadAsync(new Proto.GeoResourceNameRequest { FileName = fileName }, callOptions),
            response => new GeoResourceContent(response.FileName, response.Content.ToByteArray()),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<GeoResourceDto> UploadGeoResourceAsync(
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        using var memory = new MemoryStream();
        await content.CopyToAsync(memory, cancellationToken);

        return await ExecuteUnaryAsync(
            "Upload",
            callOptions => client.UploadAsync(
                new Proto.UploadGeoResourceRequest
                {
                    FileName = fileName,
                    Content = ByteString.CopyFrom(memory.ToArray())
                },
                callOptions),
            NodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<GeoResourceDto> RenameGeoResourceAsync(
        string fileName,
        RenameGeoResourceRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "Rename",
            callOptions => client.RenameAsync(
                new Proto.RenameGeoResourceRequest
                {
                    FileName = fileName,
                    NewFileName = request.FileName
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
