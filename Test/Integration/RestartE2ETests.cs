using System.Net;
using XRayne.Contracts.Enums;
using XRayne.Test.Infrastructure;

namespace XRayne.Test.Integration;

[Collection(PostgresCollection.Name)]
public sealed class RestartE2ETests
{
    private readonly PostgresFixture _postgres;

    public RestartE2ETests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    private async Task<XRayneWebApplicationFactory> CreateFactoryAsync()
    {
        await _postgres.ResetAsync();
        return new XRayneWebApplicationFactory(_postgres.ConnectionString);
    }

    [Fact]
    public async Task Restart_Endpoint_Returns202_ForSuperAdmin()
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsync("/api/settings/panel/restart", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task Restart_RequiresSuperAdmin_Returns403_Otherwise()
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminPermission.ViewLogs);

        var response = await client.PostAsync("/api/settings/panel/restart", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
