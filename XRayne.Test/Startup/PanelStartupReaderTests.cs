using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging.Abstractions;
using XRayne.Api.Auth;
using XRayne.Repositories.Panel;
using XRayne.Test.Infrastructure;

namespace XRayne.Test.Startup;

[Collection(PostgresCollection.Name)]
public sealed class PanelStartupReaderTests
{
    private readonly PostgresFixture _postgres;

    public PanelStartupReaderTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task TryRead_OnEmptyDb_ReturnsNull()
    {
        await _postgres.ResetAsync();

        var result = PanelStartupReader.TryRead(_postgres.ConnectionString);

        result.Should().BeNull();
    }

    [Fact]
    public async Task TryRead_OnDefaultsRow_ReturnsValues_AndShouldOverrideKestrelIsFalse()
    {
        await _postgres.ResetAsync();
        await using var context = await AppDbContextFactory.CreateAsync(_postgres.ConnectionString);
        var repo = new PanelSettingsRepository(context);
        await repo.GetAsync();

        var result = PanelStartupReader.TryRead(_postgres.ConnectionString);

        result.Should().NotBeNull();
        result!.Port.Should().Be(5097);
        result.WebBasePath.Should().Be("/");
        PanelStartupReader.ShouldOverrideKestrel(result).Should().BeFalse();
    }

    [Fact]
    public async Task TryRead_WithBindIpAndPortSet_ReturnsConfigured_AndOverrideTrue()
    {
        await _postgres.ResetAsync();
        await using var context = await AppDbContextFactory.CreateAsync(_postgres.ConnectionString);
        var repo = new PanelSettingsRepository(context);
        var settings = await repo.GetAsync();
        settings.BindIp = "127.0.0.1";
        settings.Port = 5555;
        await repo.UpdateAsync(settings);

        var result = PanelStartupReader.TryRead(_postgres.ConnectionString);

        result.Should().NotBeNull();
        result!.BindIp.Should().Be("127.0.0.1");
        result.Port.Should().Be(5555);
        PanelStartupReader.ShouldOverrideKestrel(result).Should().BeTrue();
    }

    [Fact]
    public async Task TryRead_LoadsAllStartupFields()
    {
        await _postgres.ResetAsync();
        await using var context = await AppDbContextFactory.CreateAsync(_postgres.ConnectionString);
        var repo = new PanelSettingsRepository(context);
        var settings = await repo.GetAsync();
        settings.Domain = "https://panel.example";
        settings.WebBasePath = "/admin/";
        settings.SessionLifetimeMinutes = 60;
        settings.TrustedProxyCidrs = "10.0.0.0/8";
        await repo.UpdateAsync(settings);

        var result = PanelStartupReader.TryRead(_postgres.ConnectionString);

        result.Should().NotBeNull();
        result!.Domain.Should().Be("https://panel.example");
        result.WebBasePath.Should().Be("/admin/");
        result.SessionLifetimeMinutes.Should().Be(60);
        result.TrustedProxyCidrs.Should().Be("10.0.0.0/8");
    }

    [Fact]
    public void TryRead_OnInvalidConnectionString_ReturnsNull()
    {
        var result = PanelStartupReader.TryRead("Host=invalid-host;Database=none;Username=x;Password=x");

        result.Should().BeNull();
    }

    [Fact]
    public void ApplyKestrel_WithBindIpAndPort_DoesNotThrow()
    {
        var kestrel = new KestrelServerOptions();
        var settings = new PanelStartupSettings("127.0.0.1", 9999, null, null, null, "/", 7200, null);

        var act = () => PanelStartupReader.ApplyKestrel(kestrel, settings, NullLogger.Instance);

        act.Should().NotThrow();
    }

    [Fact]
    public void ApplyKestrel_WithMissingCertFiles_FallsBackToHttp()
    {
        var kestrel = new KestrelServerOptions();
        var settings = new PanelStartupSettings("127.0.0.1", 9999, "/missing/cert.pem", "/missing/key.pem", null, "/", 7200, null);

        var act = () => PanelStartupReader.ApplyKestrel(kestrel, settings, NullLogger.Instance);

        act.Should().NotThrow();
    }
}
