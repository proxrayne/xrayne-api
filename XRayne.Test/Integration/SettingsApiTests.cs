using System.Net;
using XRayne.Contracts.Enums;
using XRayne.Test.Infrastructure;

namespace XRayne.Test.Integration;

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
}
