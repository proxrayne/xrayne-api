using AutoMapper;
using Api.Mapping;
using Api.Requests;
using Api.Responses;
using Contracts.Configurations;
using PanelSettingsEntity = Repositories.Entities;

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
    public void Map_PanelOptions_To_PanelSettingsResponse_FillsFieldImpacts()
    {
        var source = new PanelSettings { Port = 1234 };

        var response = _mapper.Map<PanelSettingsResponse>(source);

        response.Port.Should().Be(1234);
        response.FieldImpacts.Should().ContainKey("port").WhoseValue.Should().Be(RestartImpact.FullRestart);
        response.FieldImpacts.Should().ContainKey("webBasePath").WhoseValue.Should().Be(RestartImpact.FullRestart);
        response.FieldImpacts.Should().ContainKey("domain").WhoseValue.Should().Be(RestartImpact.FullRestart);
        response.FieldImpacts.Should().ContainKey("bindIp").WhoseValue.Should().Be(RestartImpact.FullRestart);
        response.FieldImpacts.Should().ContainKey("certificatesDirectory").WhoseValue.Should().Be(RestartImpact.HotReload);
        response.FieldImpacts.Should().ContainKey("geoResourcesDirectory").WhoseValue.Should().Be(RestartImpact.HotReload);
    }

    [Fact]
    public void Map_UpdateRequest_To_PanelOptions_HandlesNullables()
    {
        var request = new UpdatePanelSettingsRequest
        {
            BindIp = null,
            Domain = "example.com",
            Port = 9090,
            PathBase = "/p/",
            TrustedProxyCidrs = null
        };

        var options = _mapper.Map<PanelSettings>(request);

        options.BindIp.Should().BeNull();
        options.Domain.Should().Be("example.com");
        options.Port.Should().Be(9090);
        options.PathBase.Should().Be("/p/");
        options.TrustedProxyCidrs.Should().BeNull();
    }

    [Fact]
    public void Map_Entity_To_PanelOptions_RoundTrip_PreservesAllFields()
    {
        var entity = new PanelSettingsEntity
        {
            BindIp = "127.0.0.1",
            Domain = "panel.local",
            Port = 5097,
            WebBasePath = "/admin/",
            SessionLifetimeMinutes = 60,
            TrustedProxyCidrs = "10.0.0.0/8",
            CertificatesDirectory = "/c",
            GeoResourcesDirectory = "/g",
            PanelCertPublicKeyPath = "/pub",
            PanelCertPrivateKeyPath = "/priv"
        };

        var options = _mapper.Map<PanelSettings>(entity);

        options.BindIp.Should().Be("127.0.0.1");
        options.Domain.Should().Be("panel.local");
        options.Port.Should().Be(5097);
        options.PathBase.Should().Be("/admin/");
        options.SessionLifetimeMinutes.Should().Be(60);
        options.TrustedProxyCidrs.Should().Be("10.0.0.0/8");
        options.CertificatesDirectory.Should().Be("/c");
        options.GeoResourcesDirectory.Should().Be("/g");
        options.CertPublicKeyPath.Should().Be("/pub");
        options.CertPrivateKeyPath.Should().Be("/priv");
    }
}
