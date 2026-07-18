using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using Node.Configurations;
using Node.Grpc;
using Node.Models;
using Xray.Config.Models;
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
        XrayConfig config,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "StartCore",
            opt => client.StartAsync(
                new Proto.StartCoreRequest()
                {
                    ConfigJson = NodeGrpcMapper.SerializeXray(config)
                }, opt),
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
        XrayConfig config,
        CancellationToken ct = default)
    {
        return ExecuteUnaryAsync(
            "RestartCore",
            callOptions => client.RestartAsync(new Proto.StartCoreRequest()
            {
                ConfigJson = NodeGrpcMapper.SerializeXray(config)
            }, callOptions),
            NodeGrpcMapper.ToDomain,
            ct);
    }

    /// <inheritdoc />
    public Task UpdateCoreConfigTemplateAsync(
        XrayConfig config,
        CancellationToken ct = default)
    {
        return ExecuteEmptyUnaryAsync(
            "UpdateCoreConfigTemplate",
            opt => client.UpdateConfigTemplateAsync(new Proto.UpdateCoreConfigTemplateRequest()
            {
                ConfigTemplateJson = NodeGrpcMapper.SerializeXray(config)
            }, opt),
            ct);
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
