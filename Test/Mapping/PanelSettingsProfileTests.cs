using AutoMapper;
using Api.Mapping;
using Api.Requests;
using Api.Responses;
using Contracts.Configurations;

namespace Test.Mapping;

public sealed class PanelSettingsProfileTests
{
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<PanelSettingsProfile>()).CreateMapper();

    [Fact]
    public void MapperConfiguration_IsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<PanelSettingsProfile>());
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_PanelSettings_To_PanelSettingsDto_CopiesBootstrapFields()
    {
        var source = new PanelSettings
        {
            BindIp = "127.0.0.1",
            Domain = "panel.example.com",
            Port = 1234,
            CertPublicKeyPath = "/certs/fullchain.pem",
            CertPrivateKeyPath = "/certs/privkey.pem",
        };

        var response = _mapper.Map<PanelSettingsDto>(source);

        response.BindIp.Should().Be("127.0.0.1");
        response.Domain.Should().Be("panel.example.com");
        response.Port.Should().Be(1234);
        response.CertPublicKeyPath.Should().Be("/certs/fullchain.pem");
        response.CertPrivateKeyPath.Should().Be("/certs/privkey.pem");
    }

    [Fact]
    public void Map_PanelSettings_To_PanelSettingsResponse_NestsSettingsDto()
    {
        var source = new PanelSettings
        {
            BindIp = "127.0.0.1",
            Domain = "panel.example.com",
            Port = 1234,
            CertPublicKeyPath = "/certs/fullchain.pem",
            CertPrivateKeyPath = "/certs/privkey.pem",
        };

        var response = _mapper.Map<PanelSettingsResponse>(source);

        response.PendingRestart.Should().BeFalse();
        response.Settings.BindIp.Should().Be("127.0.0.1");
        response.Settings.Domain.Should().Be("panel.example.com");
        response.Settings.Port.Should().Be(1234);
        response.Settings.CertPublicKeyPath.Should().Be("/certs/fullchain.pem");
        response.Settings.CertPrivateKeyPath.Should().Be("/certs/privkey.pem");
    }

    [Fact]
    public void Map_UpdateRequest_To_PanelSettings_MapsBootstrapFieldsOnly()
    {
        var request = new UpdatePanelSettingsRequest
        {
            BindIp = null,
            Domain = "example.com",
            Port = 9090,
            PathBase = "/p/",
            TrustedProxyCidrs = "10.0.0.0/8",
            CertPublicKeyPath = "/certs/fullchain.pem",
            CertPrivateKeyPath = "/certs/privkey.pem",
        };

        var options = _mapper.Map<PanelSettings>(request);

        options.BindIp.Should().BeNull();
        options.Domain.Should().Be("example.com");
        options.Port.Should().Be(9090);
        options.CertPublicKeyPath.Should().Be("/certs/fullchain.pem");
        options.CertPrivateKeyPath.Should().Be("/certs/privkey.pem");
    }
}
