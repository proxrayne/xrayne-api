using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;
using Proto = XRayne.ProtoTypes.RemoteNode.V1;

namespace Node.Services;

/// <summary>
/// Sends authenticated xray-core gRPC calls to a remote node.
/// </summary>
public sealed class NodeCoreClient : NodeGrpcClientBase, INodeCoreClient
{
    private readonly Proto.CoreService.CoreServiceClient client;

    /// <summary>
    /// Initializes a remote node core client.
    /// </summary>
    public NodeCoreClient(
        IOptions<NodeOptions> options,
        INodeGrpcChannelProvider channelProvider,
        NodeEndpoint endpoint)
        : base(options, channelProvider, endpoint)
    {
        client = new Proto.CoreService.CoreServiceClient(Channel);
    }

    /// <inheritdoc />
    public Task<CoreStatusResponse> GetCoreStatusAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "GetCoreStatus",
            callOptions => client.GetStatusAsync(new Empty(), callOptions),
            NodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<InstallCoreResponse> InstallCoreAsync(
        InstallCoreRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "InstallCore",
            callOptions => client.InstallAsync(ToProto(request), callOptions),
            NodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<InstallCoreStatusResponse> GetInstallCoreStatusAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "GetInstallCoreStatus",
            callOptions => client.GetInstallStatusAsync(new Proto.GetInstallCoreStatusRequest { JobId = jobId }, callOptions),
            NodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationAcceptedResponse> StartCoreAsync(
        StartCoreRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "StartCore",
            callOptions => client.StartAsync(NodeGrpcMapper.ToProto(request), callOptions),
            NodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationAcceptedResponse> StopCoreAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "StopCore",
            callOptions => client.StopAsync(new Empty(), callOptions),
            NodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationAcceptedResponse> RestartCoreAsync(
        StartCoreRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "RestartCore",
            callOptions => client.RestartAsync(NodeGrpcMapper.ToProto(request), callOptions),
            NodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateCoreConfigTemplateAsync(
        UpdateCoreConfigTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "UpdateCoreConfigTemplate",
            callOptions => client.UpdateConfigTemplateAsync(NodeGrpcMapper.ToProto(request), callOptions),
            cancellationToken);
    }

    private static Proto.InstallCoreRequest ToProto(InstallCoreRequest request)
    {
        var response = new Proto.InstallCoreRequest();
        if (!string.IsNullOrWhiteSpace(request.Version))
        {
            response.Version = request.Version;
        }

        return response;
    }
}
