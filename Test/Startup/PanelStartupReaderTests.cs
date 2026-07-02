using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging.Abstractions;
using XRayne.Api.Auth;
using XRayne.Contracts.Configurations;

namespace XRayne.Test.Startup;

public sealed class PanelStartupReaderTests
{
    [Fact]
    public void ShouldOverrideKestrel_OnDefaultSettings_ReturnsFalse()
    {
        var settings = new PanelSettings { Port = 5097 };

        PanelStartupReader.ShouldOverrideKestrel(settings).Should().BeFalse();
    }

    [Fact]
    public void ShouldOverrideKestrel_WithBindIpAndPortSet_ReturnsTrue()
    {
        var settings = new PanelSettings
        {
            BindIp = "127.0.0.1",
            Port = 5555,
        };

        PanelStartupReader.ShouldOverrideKestrel(settings).Should().BeTrue();
    }

    [Fact]
    public void ApplyKestrel_WithBindIpAndPort_DoesNotThrow()
    {
        var kestrel = new KestrelServerOptions();
        var settings = new PanelSettings
        {
            BindIp = "127.0.0.1",
            Port = 9999,
        };

        var act = () => PanelStartupReader.ApplyKestrel(kestrel, settings, NullLogger.Instance);

        act.Should().NotThrow();
    }

    [Fact]
    public void ApplyKestrel_WithMissingCertFiles_FallsBackToHttp()
    {
        var kestrel = new KestrelServerOptions();
        var settings = new PanelSettings
        {
            BindIp = "127.0.0.1",
            Port = 9999,
            CertPublicKeyPath = "/missing/cert.pem",
            CertPrivateKeyPath = "/missing/key.pem",
        };

        var act = () => PanelStartupReader.ApplyKestrel(kestrel, settings, NullLogger.Instance);

        act.Should().NotThrow();
    }
}
