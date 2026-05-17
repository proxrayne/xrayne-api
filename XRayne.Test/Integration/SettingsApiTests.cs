using System.Net;
using System.Net.Http.Json;
using XRayne.Api.Requests;
using XRayne.Api.Responses;
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
    public async Task Get_WithoutAuth_Returns401()
    {
        await using var factory = await CreateFactoryAsync();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/settings/panel");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_AsRegularAdmin_Returns403()
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync(AdminPermission.ViewLogs);

        var response = await client.GetAsync("/api/settings/panel");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Get_AsSuperAdmin_ReturnsDefaults()
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetFromJsonAsync<PanelSettingsResponse>("/api/settings/panel");

        response.Should().NotBeNull();
        response!.Port.Should().Be(5097);
        response.WebBasePath.Should().Be("/");
        response.PendingRestart.Should().BeFalse();
        response.FieldImpacts.Should().ContainKey("port");
    }

    [Fact]
    public async Task Put_HotReloadField_ReturnsRequiresRestartFalse_AndPersists()
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync();

        var put = await client.PutAsJsonAsync("/api/settings/panel", new UpdatePanelSettingsRequest
        {
            Port = 5097,
            CertificatesDirectory = "/custom/certs"
        });
        put.EnsureSuccessStatusCode();

        var result = await put.Content.ReadFromJsonAsync<UpdatePanelSettingsResponse>();
        result.Should().NotBeNull();
        result!.RequiresRestart.Should().BeFalse();
        result.ChangedFields.Should().Contain("certificatesDirectory");
        result.HotReloaded.Should().Contain("certificatesDirectory");

        var after = await client.GetFromJsonAsync<PanelSettingsResponse>("/api/settings/panel");
        after!.CertificatesDirectory.Should().Be("/custom/certs");
        after.PendingRestart.Should().BeFalse();
    }

    [Fact]
    public async Task Put_FullRestartField_ReturnsRequiresRestartTrue_AndSetsPendingRestart()
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync();

        var put = await client.PutAsJsonAsync("/api/settings/panel", new UpdatePanelSettingsRequest
        {
            Port = 5098
        });
        put.EnsureSuccessStatusCode();

        var result = await put.Content.ReadFromJsonAsync<UpdatePanelSettingsResponse>();
        result!.RequiresRestart.Should().BeTrue();
        result.HotReloaded.Should().BeEmpty();
        result.ChangedFields.Should().Contain("port");

        var after = await client.GetFromJsonAsync<PanelSettingsResponse>("/api/settings/panel");
        after!.PendingRestart.Should().BeTrue();
        after.Port.Should().Be(5098);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(65536)]
    public async Task Put_WithInvalidPort_Returns400(int port)
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync();

        var put = await client.PutAsJsonAsync("/api/settings/panel", new UpdatePanelSettingsRequest { Port = port });

        put.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("/admin")]
    [InlineData("")]
    public async Task Put_WithInvalidWebBasePath_Returns400(string path)
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync();

        var put = await client.PutAsJsonAsync("/api/settings/panel", new UpdatePanelSettingsRequest { WebBasePath = path });

        put.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("not-a-cidr")]
    [InlineData("127.0.0.1/99")]
    public async Task Put_WithInvalidCidr_Returns400(string cidrs)
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync();

        var put = await client.PutAsJsonAsync("/api/settings/panel", new UpdatePanelSettingsRequest { TrustedProxyCidrs = cidrs });

        put.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("999.999.999.999")]
    [InlineData("not-an-ip")]
    public async Task Put_WithInvalidBindIp_Returns400(string bindIp)
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync();

        var put = await client.PutAsJsonAsync("/api/settings/panel", new UpdatePanelSettingsRequest { BindIp = bindIp });

        put.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_MultipleSequential_AccumulatesPendingRestart()
    {
        await using var factory = await CreateFactoryAsync();
        var client = await factory.CreateAuthenticatedClientAsync();

        await client.PutAsJsonAsync("/api/settings/panel", new UpdatePanelSettingsRequest { Port = 5098 });
        await client.PutAsJsonAsync("/api/settings/panel", new UpdatePanelSettingsRequest { Port = 5098, CertificatesDirectory = "/c" });

        var after = await client.GetFromJsonAsync<PanelSettingsResponse>("/api/settings/panel");
        after!.PendingRestart.Should().BeTrue();
        after.CertificatesDirectory.Should().Be("/c");
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
