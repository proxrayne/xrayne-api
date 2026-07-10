using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Contracts.Enums;
using Test.Infrastructure;

namespace Test.Integration;

[Collection(PostgresCollection.Name)]
public sealed class SettingsApiTests
{
    private readonly PostgresFixture _postgres;

    public SettingsApiTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    private async Task<XRayneWebApplicationFactory> CreateFactoryAsync()
    {
        await _postgres.ResetAsync();
        return new XRayneWebApplicationFactory(_postgres.ConnectionString);
    }

    [Fact]
    public async Task Restart_AsSuperAdmin_Returns202()
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsync("/api/settings/panel/restart", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task Restart_AsRegularAdmin_Returns403()
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminPermission.ViewLogs);

        var response = await client.PostAsync("/api/settings/panel/restart", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Webhooks_ReturnSnakeCaseEnumValues()
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/settings/app/webhooks",
            new
            {
                url = "https://example.com/webhook",
                events = new[] { "user_created", "traffic_percent_threshold_reached" },
                secret = "secret",
                retryAttempts = 2,
                retryIntervalSeconds = 30,
                subscriptionExpirationThresholdHours = new[] { 24 },
                trafficThresholdPercents = new[] { 80 }
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var events = json.RootElement.GetProperty("events")
            .EnumerateArray()
            .Select(item => item.GetString())
            .ToArray();

        events.Should().BeEquivalentTo(["user_created", "traffic_percent_threshold_reached"]);
    }

    [Fact]
    public async Task Webhooks_AcceptNumericEnumValues()
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/settings/app/webhooks",
            new
            {
                url = "https://example.com/webhook",
                events = new[] { 1 },
                retryAttempts = 2,
                retryIntervalSeconds = 30,
                subscriptionExpirationThresholdHours = Array.Empty<int>(),
                trafficThresholdPercents = Array.Empty<int>()
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
