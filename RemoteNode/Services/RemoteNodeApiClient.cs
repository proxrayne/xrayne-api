using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Models;

namespace RemoteNode.Services;

/// <summary>
/// Sends authenticated request/response API calls to a remote node.
/// </summary>
public sealed class RemoteNodeApiClient(
    IHttpClientFactory httpClientFactory,
    IOptions<RemoteNodeOptions> options,
    RemoteNodeEndpoint endpoint)
    : RemoteNodeHttpClientBase(httpClientFactory, options, endpoint), IRemoteNodeApiClient
{
    /// <inheritdoc />
    public Task<NodePingResponse> PingAsync(CancellationToken cancellationToken = default)
        => SendJsonAsync<NodePingResponse>(HttpMethod.Get, "api/ping", null, cancellationToken);

    /// <inheritdoc />
    public Task<RemoteLogSnapshotResponse> GetLogsAsync(
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<RemoteLogSnapshotResponse>(
            HttpMethod.Get,
            BuildLogsPath("api/logs", limit),
            null,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<SystemStatusResponse> GetSystemStatusAsync(CancellationToken cancellationToken = default)
        => SendJsonAsync<SystemStatusResponse>(HttpMethod.Get, "api/system/status", null, cancellationToken);

    /// <inheritdoc />
    public Task<CoreStatusResponse> GetCoreStatusAsync(CancellationToken cancellationToken = default)
        => SendJsonAsync<CoreStatusResponse>(HttpMethod.Get, "api/core/status", null, cancellationToken);

    /// <inheritdoc />
    public Task<InstallCoreResponse> InstallCoreAsync(
        InstallCoreRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<InstallCoreResponse>(HttpMethod.Post, "api/core/install", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<InstallCoreStatusResponse> GetInstallCoreStatusAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<InstallCoreStatusResponse>(
            HttpMethod.Get,
            $"api/core/install/{Uri.EscapeDataString(jobId)}/status",
            null,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationAcceptedResponse> StartCoreAsync(
        StartCoreRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<OperationAcceptedResponse>(HttpMethod.Post, "api/core/start", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationAcceptedResponse> StopCoreAsync(CancellationToken cancellationToken = default)
        => SendJsonAsync<OperationAcceptedResponse>(HttpMethod.Post, "api/core/stop", null, cancellationToken);

    /// <inheritdoc />
    public Task<OperationAcceptedResponse> RestartCoreAsync(
        StartCoreRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<OperationAcceptedResponse>(HttpMethod.Post, "api/core/restart", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task UpdateCoreConfigTemplateAsync(
        UpdateCoreConfigTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendNoContentAsync(HttpMethod.Put, "api/core/config-template", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationAcceptedResponse> RestartRuntimeAsync(CancellationToken cancellationToken = default)
        => SendJsonAsync<OperationAcceptedResponse>(HttpMethod.Post, "api/runtime/restart", null, cancellationToken);

    /// <inheritdoc />
    public Task AddInboundAsync(SyncInboundRequest request, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Post, "api/core/inbounds", request, cancellationToken);

    /// <inheritdoc />
    public Task UpdateInboundAsync(
        long id,
        SyncInboundRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendNoContentAsync(
            HttpMethod.Put,
            $"api/core/inbounds/{id}",
            request,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteInboundAsync(long id, CancellationToken cancellationToken = default)
    {
        return SendNoContentAsync(
            HttpMethod.Delete,
            $"api/core/inbounds/{id}",
            null,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task AddOutboundAsync(SyncOutboundRequest request, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Post, "api/core/outbounds", request, cancellationToken);

    /// <inheritdoc />
    public Task UpdateOutboundAsync(
        long id,
        SyncOutboundRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendNoContentAsync(
            HttpMethod.Put,
            $"api/core/outbounds/{id}",
            request,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteOutboundAsync(long id, CancellationToken cancellationToken = default)
    {
        return SendNoContentAsync(
            HttpMethod.Delete,
            $"api/core/outbounds/{id}",
            null,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task SyncRoutingRulesAsync(
        SyncRoutingRulesRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendNoContentAsync(HttpMethod.Put, "api/core/routing/rules", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<GeoResourceDto>> GetGeoResourcesAsync(CancellationToken cancellationToken = default)
        => SendJsonAsync<List<GeoResourceDto>>(HttpMethod.Get, "api/geo-resources", null, cancellationToken);

    /// <inheritdoc />
    public async Task<GeoResourceContent> DownloadGeoResourceAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var path = $"api/geo-resources/{Uri.EscapeDataString(fileName)}/content";
        using var request = CreateRequest(HttpMethod.Get, path);

        using var response = await SendAsync(
            StandardClient,
            request,
            path,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        await EnsureSuccessAsync(response, path, cancellationToken);

        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        return new GeoResourceContent(fileName, content);
    }

    /// <inheritdoc />
    public async Task<GeoResourceDto> UploadGeoResourceAsync(
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var path = $"api/geo-resources/{Uri.EscapeDataString(fileName)}";
        using var request = CreateRequest(HttpMethod.Put, path);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StreamContent(content);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        using var response = await SendAsync(
            StandardClient,
            request,
            path,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        await EnsureSuccessAsync(response, path, cancellationToken);

        return await ReadJsonAsync<GeoResourceDto>(response.Content, path, cancellationToken);
    }

    /// <inheritdoc />
    public Task<GeoResourceDto> RenameGeoResourceAsync(
        string fileName,
        RenameGeoResourceRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<GeoResourceDto>(
            HttpMethod.Post,
            $"api/geo-resources/{Uri.EscapeDataString(fileName)}/rename",
            request,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteGeoResourceAsync(string fileName, CancellationToken cancellationToken = default)
    {
        return SendNoContentAsync(
            HttpMethod.Delete,
            $"api/geo-resources/{Uri.EscapeDataString(fileName)}",
            null,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<List<CertificateDto>> GetCertificatesAsync(CancellationToken cancellationToken = default)
        => SendJsonAsync<List<CertificateDto>>(HttpMethod.Get, "api/certificates", null, cancellationToken);

    /// <inheritdoc />
    public Task<CertificateDto> IssueCertificateAsync(
        IssueCertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<CertificateDto>(HttpMethod.Post, "api/certificates/issue", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<CertificateDto> UploadCertificateAsync(
        UploadCertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<CertificateDto>(HttpMethod.Post, "api/certificates/upload", request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<CertificateDto> RenewCertificateAsync(string domain, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync<CertificateDto>(
            HttpMethod.Post,
            $"api/certificates/{Uri.EscapeDataString(domain)}/renew",
            null,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteCertificateAsync(string domain, CancellationToken cancellationToken = default)
    {
        return SendNoContentAsync(
            HttpMethod.Delete,
            $"api/certificates/{Uri.EscapeDataString(domain)}",
            null,
            cancellationToken);
    }

    private async Task<T> SendJsonAsync<T>(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, path);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (body is not null)
        {
            request.Content = CreateJsonContent(body);
        }

        using var response = await SendAsync(
            StandardClient,
            request,
            path,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        await EnsureSuccessAsync(response, path, cancellationToken);

        return await ReadJsonAsync<T>(response.Content, path, cancellationToken);
    }

    private async Task SendNoContentAsync(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, path);
        if (body is not null)
        {
            request.Content = CreateJsonContent(body);
        }

        using var response = await SendAsync(
            StandardClient,
            request,
            path,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        await EnsureSuccessAsync(response, path, cancellationToken);
    }
}
