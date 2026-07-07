using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Exceptions;
using RemoteNode.Models;
using RemoteNode.Parsing;

namespace RemoteNode.Services;

/// <summary>
/// Sends authenticated HTTP and SSE requests to a remote node API.
/// </summary>
public sealed class RemoteNodeApiClient(
    IHttpClientFactory httpClientFactory,
    IOptions<RemoteNodeOptions> options,
    RemoteNodeEndpoint endpoint) : IRemoteNodeApiClient
{
    private const string ClientName = "remote-node";
    private const int MaxErrorBodyLength = 2048;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public Task<NodePingResponse> PingAsync(CancellationToken cancellationToken = default)
        => SendJsonAsync<NodePingResponse>(HttpMethod.Get, "api/ping", null, cancellationToken);

    /// <inheritdoc />
    public async IAsyncEnumerable<NodeConnectionEvent> ConnectStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var connectionEvent in ReadServerSentEventsAsync<NodeConnectionEvent>(
                           "api/connect",
                           cancellationToken))
        {
            yield return connectionEvent;
        }
    }

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
    public async IAsyncEnumerable<RemoteLogStreamEvent> LogStreamAsync(
        int? limit = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var logEvent in ReadServerSentEventsAsync<RemoteLogStreamEvent>(
                           BuildLogsPath("api/logs/stream", limit),
                           cancellationToken))
        {
            yield return logEvent;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<CoreStatusResponse> CoreStatusStreamAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var state in ReadServerSentEventsAsync<CoreStatusResponse>(
                           "api/core/status/stream",
                           cancellationToken))
        {
            yield return state;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<InstallCoreStatusResponse> InstallCoreStatusStreamAsync(
        string jobId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var state in ReadServerSentEventsAsync<InstallCoreStatusResponse>(
                           $"api/core/install/{Uri.EscapeDataString(jobId)}/stream",
                           cancellationToken))
        {
            yield return state;
        }
    }

    private async IAsyncEnumerable<T> ReadServerSentEventsAsync<T>(
        string path,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var httpClient = CreateStreamClient();
        using var request = CreateRequest(HttpMethod.Get, path);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var response = await SendAsync(
            httpClient,
            request,
            path,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        await EnsureSuccessAsync(response, path, cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        await foreach (var payload in ServerSentEventParser.ReadDataAsync(reader, cancellationToken))
        {
            T? value;
            try
            {
                value = JsonSerializer.Deserialize<T>(payload, JsonOptions);
            }
            catch (JsonException exception)
            {
                throw new RemoteNodeProtocolException(endpoint.NodeId, path, "Invalid SSE JSON payload.", exception);
            }

            if (value is null)
            {
                throw new RemoteNodeProtocolException(endpoint.NodeId, path, "Empty SSE JSON payload.");
            }

            yield return value;
        }
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
    public Task<OperationAcceptedResponse> RestartRuntimeAsync(CancellationToken cancellationToken = default)
        => SendJsonAsync<OperationAcceptedResponse>(HttpMethod.Post, "api/runtime/restart", null, cancellationToken);

    /// <inheritdoc />
    public Task AddInboundAsync(SyncInboundRequest request, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Post, "api/core/inbounds", request, cancellationToken);

    /// <inheritdoc />
    public Task UpdateInboundAsync(
        string tag,
        SyncInboundRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendNoContentAsync(
            HttpMethod.Put,
            $"api/core/inbounds/{Uri.EscapeDataString(tag)}",
            request,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteInboundAsync(string tag, CancellationToken cancellationToken = default)
    {
        return SendNoContentAsync(
            HttpMethod.Delete,
            $"api/core/inbounds/{Uri.EscapeDataString(tag)}",
            null,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task AddOutboundAsync(SyncOutboundRequest request, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Post, "api/core/outbounds", request, cancellationToken);

    /// <inheritdoc />
    public Task UpdateOutboundAsync(
        string tag,
        SyncOutboundRequest request,
        CancellationToken cancellationToken = default)
    {
        return SendNoContentAsync(
            HttpMethod.Put,
            $"api/core/outbounds/{Uri.EscapeDataString(tag)}",
            request,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteOutboundAsync(string tag, CancellationToken cancellationToken = default)
    {
        return SendNoContentAsync(
            HttpMethod.Delete,
            $"api/core/outbounds/{Uri.EscapeDataString(tag)}",
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
        using var httpClient = CreateStandardClient();
        using var request = CreateRequest(method, path);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        using var response = await SendAsync(
            httpClient,
            request,
            path,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        await EnsureSuccessAsync(response, path, cancellationToken);

        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken)
                ?? throw new RemoteNodeProtocolException(endpoint.NodeId, path, "Empty JSON response.");
        }
        catch (JsonException exception)
        {
            throw new RemoteNodeProtocolException(endpoint.NodeId, path, "Invalid JSON response.", exception);
        }
    }

    private async Task SendNoContentAsync(
        HttpMethod method,
        string path,
        object? body,
        CancellationToken cancellationToken)
    {
        using var httpClient = CreateStandardClient();
        using var request = CreateRequest(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        using var response = await SendAsync(
            httpClient,
            request,
            path,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        await EnsureSuccessAsync(response, path, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpClient httpClient,
        HttpRequestMessage request,
        string path,
        HttpCompletionOption completionOption,
        CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.SendAsync(request, completionOption, cancellationToken);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new RemoteNodeTimeoutException(endpoint.NodeId, path, exception);
        }
        catch (HttpRequestException exception)
        {
            throw new RemoteNodeUnavailableException(endpoint.NodeId, path, exception);
        }
    }

    private async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string path,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new RemoteNodeUnauthorizedException(endpoint.NodeId, path, response.StatusCode);
        }

        var body = await ReadErrorBodyAsync(response, cancellationToken);
        throw new RemoteNodeHttpException(endpoint.NodeId, path, response.StatusCode, body);
    }

    private static async Task<string?> ReadErrorBodyAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        return body.Length <= MaxErrorBodyLength ? body : body[..MaxErrorBodyLength];
    }

    private HttpClient CreateStandardClient()
    {
        var httpClient = httpClientFactory.CreateClient(ClientName);
        httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.Value.PingTimeoutSeconds));

        return httpClient;
    }

    private HttpClient CreateStreamClient()
    {
        var httpClient = httpClientFactory.CreateClient(ClientName);
        httpClient.Timeout = Timeout.InfiniteTimeSpan;

        return httpClient;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, new UriBuilder("https", endpoint.Address, endpoint.ApiPort, path).Uri);
        request.Headers.Add("X-Node-Api-Key", endpoint.ApiKey);

        return request;
    }

    private static string BuildLogsPath(string path, int? limit) => limit is null ? path : QueryHelpers.AddQueryString(path, "limit", ((int)limit).ToString());
}
