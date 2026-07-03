using Microsoft.Extensions.Configuration;
using Contracts.Configurations;

namespace Test.Configurations;

public sealed class PanelSettingsTests
{
    [Fact]
    public void Parse_UsesEnvBackedPanelKeys()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IP"] = "127.0.0.1",
                ["DOMAIN"] = "panel.example.com",
                ["PORT"] = "9443",
                ["CERT_PUBLIC_KEY_PATH"] = "/certs/fullchain.pem",
                ["CERT_PRIVATE_KEY_PATH"] = "/certs/privkey.pem",
            })
            .Build();

        var settings = PanelSettings.Parse(configuration);

        settings.BindIp.Should().Be("127.0.0.1");
        settings.Domain.Should().Be("panel.example.com");
        settings.Port.Should().Be(9443);
        settings.CertPublicKeyPath.Should().Be("/certs/fullchain.pem");
        settings.CertPrivateKeyPath.Should().Be("/certs/privkey.pem");
    }

    [Fact]
    public void Parse_IgnoresLegacyPanelKeysExceptConfigurationCaseInsensitiveDomain()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BindIp"] = "127.0.0.1",
                ["Domain"] = "legacy.example.com",
                ["API_PORT"] = "9443",
                ["PathBase"] = "/legacy/",
                ["Cert:PublicKeyPath"] = "/legacy/fullchain.pem",
                ["Cert:PrivateKeyPath"] = "/legacy/privkey.pem",
            })
            .Build();

        var settings = PanelSettings.Parse(configuration);

        settings.BindIp.Should().BeNull();
        settings.Domain.Should().Be("legacy.example.com");
        settings.Port.Should().Be(5097);
        settings.CertPublicKeyPath.Should().BeNull();
        settings.CertPrivateKeyPath.Should().BeNull();
        typeof(PanelSettings).GetProperty("PathBase").Should().BeNull();
    }
}
