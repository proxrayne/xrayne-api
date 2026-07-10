using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Grpc;
using RemoteNode.Models;
using Proto = XRayne.ProtoTypes.RemoteNode.V1;

namespace RemoteNode.Services;

/// <summary>
/// Sends authenticated request/response gRPC calls to a remote node.
/// </summary>
public sealed class RemoteNodeApiClient(
    IOptions<RemoteNodeOptions> options,
    IRemoteNodeGrpcChannelProvider channelProvider,
    RemoteNodeEndpoint endpoint)
    : RemoteNodeGrpcClientBase(options, channelProvider, endpoint), IRemoteNodeApiClient
{
    /// <inheritdoc />
    public Task<NodePingResponse> PingAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "Ping",
            callOptions => Client.PingAsync(new Empty(), callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<RemoteLogSnapshotResponse> GetLogsAsync(
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "GetLogs",
            callOptions =>
            {
                var request = new Proto.LogStreamRequest();
                if (limit is not null)
                {
                    request.Limit = limit.Value;
                }

                return Client.GetLogsAsync(request, callOptions);
            },
            response => new RemoteLogSnapshotResponse(response.Limit, [.. response.Entries.Select(RemoteNodeGrpcMapper.ToDomain)]),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<SystemStatusResponse> GetSystemStatusAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "GetSystemStatus",
            callOptions => Client.GetSystemStatusAsync(new Empty(), callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<CoreStatusResponse> GetCoreStatusAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "GetCoreStatus",
            callOptions => Client.GetCoreStatusAsync(new Empty(), callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<InstallCoreResponse> InstallCoreAsync(
        InstallCoreRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "InstallCore",
            callOptions => Client.InstallCoreAsync(ToProto(request), callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<InstallCoreStatusResponse> GetInstallCoreStatusAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "GetInstallCoreStatus",
            callOptions => Client.GetInstallCoreStatusAsync(new Proto.GetInstallCoreStatusRequest { JobId = jobId }, callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationAcceptedResponse> StartCoreAsync(
        StartCoreRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "StartCore",
            callOptions => Client.StartCoreAsync(RemoteNodeGrpcMapper.ToProto(request), callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationAcceptedResponse> StopCoreAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "StopCore",
            callOptions => Client.StopCoreAsync(new Empty(), callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationAcceptedResponse> RestartCoreAsync(
        StartCoreRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "RestartCore",
            callOptions => Client.RestartCoreAsync(RemoteNodeGrpcMapper.ToProto(request), callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateCoreConfigTemplateAsync(
        UpdateCoreConfigTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "UpdateCoreConfigTemplate",
            callOptions => Client.UpdateCoreConfigTemplateAsync(RemoteNodeGrpcMapper.ToProto(request), callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationAcceptedResponse> RestartRuntimeAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "RestartRuntime",
            callOptions => Client.RestartRuntimeAsync(new Empty(), callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task AddInboundAsync(SyncInboundRequest request, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "AddInbound",
            callOptions => Client.AddInboundAsync(RemoteNodeGrpcMapper.ToProto(request), callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateInboundAsync(
        long id,
        SyncInboundRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "UpdateInbound",
            callOptions => Client.UpdateInboundAsync(
                new Proto.UpdateInboundRequest
                {
                    Id = id,
                    Request = RemoteNodeGrpcMapper.ToProto(request)
                },
                callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteInboundAsync(long id, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "DeleteInbound",
            callOptions => Client.DeleteInboundAsync(new Proto.DeleteManagedSliceRequest { Id = id }, callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task AddOutboundAsync(SyncOutboundRequest request, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "AddOutbound",
            callOptions => Client.AddOutboundAsync(RemoteNodeGrpcMapper.ToProto(request), callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateOutboundAsync(
        long id,
        SyncOutboundRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "UpdateOutbound",
            callOptions => Client.UpdateOutboundAsync(
                new Proto.UpdateOutboundRequest
                {
                    Id = id,
                    Request = RemoteNodeGrpcMapper.ToProto(request)
                },
                callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteOutboundAsync(long id, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "DeleteOutbound",
            callOptions => Client.DeleteOutboundAsync(new Proto.DeleteManagedSliceRequest { Id = id }, callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task SyncRoutingRulesAsync(
        SyncRoutingRulesRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "SyncRoutingRules",
            callOptions => Client.SyncRoutingRulesAsync(RemoteNodeGrpcMapper.ToProto(request), callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<GeoResourceDto>> GetGeoResourcesAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "ListGeoResources",
            callOptions => Client.ListGeoResourcesAsync(new Empty(), callOptions),
            response => response.Items.Select(RemoteNodeGrpcMapper.ToDomain).ToList(),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<GeoResourceContent> DownloadGeoResourceAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "DownloadGeoResource",
            callOptions => Client.DownloadGeoResourceAsync(new Proto.GeoResourceNameRequest { FileName = fileName }, callOptions),
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
            "UploadGeoResource",
            callOptions => Client.UploadGeoResourceAsync(
                new Proto.UploadGeoResourceRequest
                {
                    FileName = fileName,
                    Content = ByteString.CopyFrom(memory.ToArray())
                },
                callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<GeoResourceDto> RenameGeoResourceAsync(
        string fileName,
        RenameGeoResourceRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "RenameGeoResource",
            callOptions => Client.RenameGeoResourceAsync(
                new Proto.RenameGeoResourceRequest
                {
                    FileName = fileName,
                    NewFileName = request.FileName
                },
                callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteGeoResourceAsync(string fileName, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "DeleteGeoResource",
            callOptions => Client.DeleteGeoResourceAsync(new Proto.GeoResourceNameRequest { FileName = fileName }, callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<CertificateDto>> GetCertificatesAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "ListCertificates",
            callOptions => Client.ListCertificatesAsync(new Empty(), callOptions),
            response => response.Items.Select(RemoteNodeGrpcMapper.ToDomain).ToList(),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<CertificateDto> IssueCertificateAsync(
        IssueCertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "IssueCertificate",
            callOptions => Client.IssueCertificateAsync(new Proto.IssueCertificateRequest { Domain = request.Domain }, callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<CertificateDto> UploadCertificateAsync(
        UploadCertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "UploadCertificate",
            callOptions => Client.UploadCertificateAsync(
                new Proto.UploadCertificateRequest
                {
                    Domain = request.Domain,
                    CertificateFile = request.CertificateFile,
                    PrivateKeyFile = request.PrivateKeyFile
                },
                callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<CertificateDto> RenewCertificateAsync(string domain, CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "RenewCertificate",
            callOptions => Client.RenewCertificateAsync(new Proto.CertificateDomainRequest { Domain = domain }, callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteCertificateAsync(string domain, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "DeleteCertificate",
            callOptions => Client.DeleteCertificateAsync(new Proto.CertificateDomainRequest { Domain = domain }, callOptions),
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
