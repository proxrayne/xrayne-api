using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Exceptions;
using RemoteNode.Models;
using Xray.Config.Models;

namespace RemoteNode.Services;

/// <summary>
/// Provides shared HTTP transport behavior for remote node clients.
/// </summary>
public abstract class RemoteNodeHttpClientBase
{
    private const string ClientName = "remote-node";
    private const int MaxErrorBodyLength = 2048;

    /// <summary>
    /// Initializes a remote node HTTP client transport.
    /// </summary>
    protected RemoteNodeHttpClientBase(
        IHttpClientFactory httpClientFactory,
        IOptions<RemoteNodeOptions> options,
        RemoteNodeEndpoint endpoint)
    {
        Endpoint = endpoint;
        JsonOptions = CreateJsonOptions();
        StandardClient = httpClientFactory.CreateClient(ClientName);
        StandardClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.Value.PingTimeoutSeconds));
        StreamClient = httpClientFactory.CreateClient(ClientName);
        StreamClient.Timeout = Timeout.InfiniteTimeSpan;
    }

    /// <summary>
    /// Gets the remote node endpoint.
    /// </summary>
    protected RemoteNodeEndpoint Endpoint { get; }

    /// <summary>
    /// Gets JSON options shared by remote node protocol clients.
    /// </summary>
    protected JsonSerializerOptions JsonOptions { get; }

    /// <summary>
    /// Gets the cached client for regular request/response calls.
    /// </summary>
    protected HttpClient StandardClient { get; }

    /// <summary>
    /// Gets the cached client for long-lived stream calls.
    /// </summary>
    protected HttpClient StreamClient { get; }

    /// <summary>
    /// Creates an authenticated HTTP request.
    /// </summary>
    protected HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, BuildUri(path));
        request.Headers.Add("X-Node-Api-Key", Endpoint.ApiKey);

        return request;
    }

    /// <summary>
    /// Sends a request and maps transport failures to remote node exceptions.
    /// </summary>
    protected async Task<HttpResponseMessage> SendAsync(
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
            throw new RemoteNodeTimeoutException(Endpoint.NodeId, path, exception);
        }
        catch (HttpRequestException exception)
        {
            throw new RemoteNodeUnavailableException(Endpoint.NodeId, path, exception);
        }
    }

    /// <summary>
    /// Throws a typed remote node exception for non-success responses.
    /// </summary>
    protected async Task EnsureSuccessAsync(
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
            throw new RemoteNodeUnauthorizedException(Endpoint.NodeId, path, response.StatusCode);
        }

        var body = await ReadErrorBodyAsync(response, cancellationToken);
        throw new RemoteNodeHttpException(Endpoint.NodeId, path, response.StatusCode, body);
    }

    /// <summary>
    /// Reads a JSON response body.
    /// </summary>
    protected async Task<T> ReadJsonAsync<T>(
        HttpContent content,
        string path,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken)
                ?? throw new RemoteNodeProtocolException(Endpoint.NodeId, path, "Empty JSON response.");
        }
        catch (JsonException exception)
        {
            throw new RemoteNodeProtocolException(Endpoint.NodeId, path, "Invalid JSON response.", exception);
        }
    }

    /// <summary>
    /// Creates JSON request content with the shared serializer options.
    /// </summary>
    protected JsonContent CreateJsonContent(object body)
    {
        return JsonContent.Create(body, options: JsonOptions);
    }

    /// <summary>
    /// Builds an absolute remote node URI.
    /// </summary>
    protected Uri BuildUri(string path)
    {
        return new UriBuilder("https", Endpoint.Address, Endpoint.ApiPort, path).Uri;
    }

    /// <summary>
    /// Builds a logs API path with an optional limit query.
    /// </summary>
    protected static string BuildLogsPath(string path, int? limit)
    {
        return limit is null ? path : QueryHelpers.AddQueryString(path, "limit", limit.Value.ToString());
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        return new JsonSerializerOptions(XrayConfig.JsonSerializationOptions)
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };
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
}
