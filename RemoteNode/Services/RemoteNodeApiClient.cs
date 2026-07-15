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
    public Task<PingResponse> PingAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "Ping",
            callOptions => HealthClient.PingAsync(new Empty(), callOptions),
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

                return LogClient.GetLogsAsync(request, callOptions);
            },
            response => new RemoteLogSnapshotResponse(response.Limit, [.. response.Entries.Select(RemoteNodeGrpcMapper.ToDomain)]),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<SystemStatusResponse> GetSystemStatusAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "GetSystemStatus",
            callOptions => HealthClient.GetSystemStatusAsync(new Empty(), callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<CoreStatusResponse> GetCoreStatusAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "GetCoreStatus",
            callOptions => CoreClient.GetStatusAsync(new Empty(), callOptions),
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
            callOptions => CoreClient.InstallAsync(ToProto(request), callOptions),
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
            callOptions => CoreClient.GetInstallStatusAsync(new Proto.GetInstallCoreStatusRequest { JobId = jobId }, callOptions),
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
            callOptions => CoreClient.StartAsync(RemoteNodeGrpcMapper.ToProto(request), callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationAcceptedResponse> StopCoreAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "StopCore",
            callOptions => CoreClient.StopAsync(new Empty(), callOptions),
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
            callOptions => CoreClient.RestartAsync(RemoteNodeGrpcMapper.ToProto(request), callOptions),
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
            callOptions => CoreClient.UpdateConfigTemplateAsync(RemoteNodeGrpcMapper.ToProto(request), callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationAcceptedResponse> RestartRuntimeAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "RestartRuntime",
            callOptions => HealthClient.RestartRuntimeAsync(new Empty(), callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task AddInboundAsync(SyncInboundRequest request, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "AddInbound",
            callOptions => RuntimeConfigClient.AddInboundAsync(RemoteNodeGrpcMapper.ToProto(request), callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateInboundAsync(
        string id,
        SyncInboundRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "UpdateInbound",
            callOptions => RuntimeConfigClient.UpdateInboundAsync(
                new Proto.UpdateInboundRequest
                {
                    Id = id,
                    Request = RemoteNodeGrpcMapper.ToProto(request)
                },
                callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteInboundAsync(string id, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "DeleteInbound",
            callOptions => RuntimeConfigClient.DeleteInboundAsync(new Proto.DeleteManagedSliceRequest { Id = id }, callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task AddOutboundAsync(SyncOutboundRequest request, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "AddOutbound",
            callOptions => RuntimeConfigClient.AddOutboundAsync(RemoteNodeGrpcMapper.ToProto(request), callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateOutboundAsync(
        string id,
        SyncOutboundRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "UpdateOutbound",
            callOptions => RuntimeConfigClient.UpdateOutboundAsync(
                new Proto.UpdateOutboundRequest
                {
                    Id = id,
                    Request = RemoteNodeGrpcMapper.ToProto(request)
                },
                callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteOutboundAsync(string id, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "DeleteOutbound",
            callOptions => RuntimeConfigClient.DeleteOutboundAsync(new Proto.DeleteManagedSliceRequest { Id = id }, callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task SyncRoutingRulesAsync(
        SyncRoutingRulesRequest request,
        CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "SyncRoutingRules",
            callOptions => RuntimeConfigClient.SyncRoutingRulesAsync(RemoteNodeGrpcMapper.ToProto(request), callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<GeoResourceDto>> GetGeoResourcesAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "ListGeoResources",
            callOptions => GeoResourceClient.ListAsync(new Empty(), callOptions),
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
            callOptions => GeoResourceClient.DownloadAsync(new Proto.GeoResourceNameRequest { FileName = fileName }, callOptions),
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
            callOptions => GeoResourceClient.UploadAsync(
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
            callOptions => GeoResourceClient.RenameAsync(
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
            callOptions => GeoResourceClient.DeleteAsync(new Proto.GeoResourceNameRequest { FileName = fileName }, callOptions),
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<CertificateDto>> GetCertificatesAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteUnaryAsync(
            "ListCertificates",
            callOptions => CertificateClient.ListAsync(new Empty(), callOptions),
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
            callOptions => CertificateClient.IssueAsync(new Proto.IssueCertificateRequest { Domain = request.Domain }, callOptions),
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
            callOptions => CertificateClient.UploadAsync(
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
            callOptions => CertificateClient.RenewAsync(new Proto.CertificateDomainRequest { Domain = domain }, callOptions),
            RemoteNodeGrpcMapper.ToDomain,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteCertificateAsync(string domain, CancellationToken cancellationToken = default)
    {
        return ExecuteEmptyUnaryAsync(
            "DeleteCertificate",
            callOptions => CertificateClient.DeleteAsync(new Proto.CertificateDomainRequest { Domain = domain }, callOptions),
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
