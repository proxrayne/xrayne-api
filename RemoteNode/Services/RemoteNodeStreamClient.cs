using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RemoteNode.Configurations;
using RemoteNode.Exceptions;
using RemoteNode.Models;
using RemoteNode.Parsing;

namespace RemoteNode.Services;

/// <summary>
/// Opens and reads long-lived SSE streams from a remote node.
/// </summary>
public sealed class RemoteNodeStreamClient(
    IHttpClientFactory httpClientFactory,
    IOptions<RemoteNodeOptions> options,
    RemoteNodeEndpoint endpoint)
    : RemoteNodeHttpClientBase(httpClientFactory, options, endpoint), IRemoteNodeStreamClient
{
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
        using var request = CreateRequest(HttpMethod.Get, path);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var response = await SendAsync(
            StreamClient,
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
                throw new RemoteNodeProtocolException(Endpoint.NodeId, path, "Invalid SSE JSON payload.", exception);
            }

            if (value is null)
            {
                throw new RemoteNodeProtocolException(Endpoint.NodeId, path, "Empty SSE JSON payload.");
            }

            yield return value;
        }
    }
}
