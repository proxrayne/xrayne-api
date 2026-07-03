using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Contracts.Configurations;
using Repositories.Entities;

namespace Infrastructure.Services;

/// <summary>
/// Verifies a node connection by calling its authenticated ping endpoint.
/// </summary>
public sealed class NodeConnectionVerifier(IOptions<NodeConnectionOptions> connectionOptions) : INodeConnectionVerifier
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<NodeConnectionVerificationResult> VerifyAsync(
        NodeEntity node,
        string apiKey,
        CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(Math.Max(1, connectionOptions.Value.PingTimeoutSeconds)),
        };

        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Add("X-Node-Api-Key", apiKey);

        var uri = new UriBuilder("https", node.Address, node.ApiPort, "api/ping").Uri;
        using var response = await httpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var ping = await JsonSerializer.DeserializeAsync<NodePingResponse>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Remote node ping response was empty.");

        return new NodeConnectionVerificationResult(ping.Core.Version, ping.Timestamp);
    }

    private sealed record NodePingResponse(
        DateTimeOffset Timestamp,
        NodeCoreStatus Core);

    private sealed record NodeCoreStatus(string? Version);
}
