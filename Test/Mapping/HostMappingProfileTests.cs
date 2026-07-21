using Api.Mapping;
using Api.Requests;
using Api.Responses;
using Api.Utilities;
using AutoMapper;
using Contracts.Enums;
using Data.Entities;
using Data.Models;
using Xray.Config.Enums;
using Xray.Config.Models;

namespace Test.Mapping;

/// <summary>
/// Tests host API mapping rules.
/// </summary>
public sealed class HostMappingProfileTests
{
    private readonly IMapper _mapper = MapperTestFactory.CreateConfiguration(
        cfg => cfg.AddProfile<HostMappingProfile>()).CreateMapper();

    [Fact]
    public void Configuration_IsValid()
    {
        var config = MapperTestFactory.CreateConfiguration(cfg => cfg.AddProfile<HostMappingProfile>());

        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void HostEntity_MapsToHostDto()
    {
        var host = new HostEntity
        {
            Id = 11,
            Name = "Primary",
            Address = "edge.example.com",
            CountryAlpha2Code = "US",
            InboundId = 7,
            Port = 443,
            ServerName = "host.example.com",
            Host = "front.example.com",
            Path = "/vless",
            Security = HostSecurity.InboundDefault,
            ALPN = ALPN.H2 | ALPN.H3,
            Fingerprint = Fingerprint.Chrome,
            FragmentTemplate = "fragment",
            NoiseTemplate = "noise",
            Enabled = true,
            IsMuxEnabled = true,
            IsUseServerNameAsHost = true,
            IsRandomUseragent = true,
            AllowIncrease = true,
            Position = 20,
            Inbound = new InboundEntity
            {
                Id = 7,
                Enabled = true,
                Config = new SocksInbound
                {
                    Tag = "socks-in",
                    Port = new Port(10808),
                    StreamSettings = new StreamSettings
                    {
                        TlsSettings = new TlsSettings { ServerName = "tls.example.com" }
                    }
                },
                Node = new NodeEntity
                {
                    Id = 3,
                    Name = "Node",
                    Address = "node.example.com",
                    Port = 22,
                    ApiPort = 8443,
                    SSHUsername = "root",
                    WorkingDirectory = "/opt/xrayne",
                    EncryptedApiKey = "encrypted",
                    ApiKeyFingerprint = "fingerprint"
                }
            }
        };

        var dto = _mapper.Map<HostDto>(host);

        dto.Id.Should().Be(11);
        dto.Name.Should().Be("Primary");
        dto.Security.Should().Be("inbound-default");
        dto.Path.Should().Be("/vless");
        dto.Alpn.Should().Equal("h2", "h3");
        dto.Fingerprint.Should().Be("chrome");
        dto.Inbound.ServerName.Should().Be("tls.example.com");
    }

    [Fact]
    public void CreateHostRequest_MapsToHostEntity()
    {
        var request = new CreateHostRequest
        {
            Name = "  Primary ",
            Address = " edge.example.com ",
            CountryAlpha2Code = "us",
            InboundId = 7,
            Port = 443,
            ServerName = " ",
            Host = " front.example.com ",
            Path = " /vless ",
            Security = null,
            Alpn = ["http/1.1", "h3"],
            Fingerprint = "chrome",
            FragmentTemplate = " fragment ",
            NoiseTemplate = " ",
            Enabled = true,
            IsMuxEnabled = true,
            IsUseServerNameAsHost = true,
            IsRandomUseragent = true,
            AllowIncrease = true
        };

        var entity = _mapper.Map<HostEntity>(request);

        entity.Name.Should().Be("Primary");
        entity.Address.Should().Be("edge.example.com");
        entity.CountryAlpha2Code.Should().Be("US");
        entity.InboundId.Should().Be(7);
        entity.Port.Should().Be(443);
        entity.ServerName.Should().BeNull();
        entity.Host.Should().Be("front.example.com");
        entity.Path.Should().Be("/vless");
        entity.Security.Should().Be(HostSecurity.InboundDefault);
        entity.ALPN.Should().Be(ALPN.H1 | ALPN.H3);
        entity.Fingerprint.Should().Be(Fingerprint.Chrome);
        entity.FragmentTemplate.Should().Be("fragment");
        entity.NoiseTemplate.Should().BeNull();
        entity.IsMuxEnabled.Should().BeTrue();
        entity.IsUseServerNameAsHost.Should().BeTrue();
        entity.IsRandomUseragent.Should().BeTrue();
        entity.AllowIncrease.Should().BeTrue();
    }

    [Fact]
    public void PatchHostRequest_MapsToHostPatch()
    {
        var request = new PatchHostRequest
        {
            Name = "  Secondary ",
            CountryAlpha2Code = "nl",
            Port = null,
            Host = " ",
            Path = " /vless ",
            Security = "tls",
            Alpn = new List<string> { "http/1.1", "h2" },
            Fingerprint = "ios",
            Enabled = false
        };

        var patch = _mapper.Map<HostPatch>(request);

        patch.Name.IsSpecified.Should().BeTrue();
        patch.Name.SpecifiedValue.Should().Be("Secondary");
        patch.Address.IsSpecified.Should().BeFalse();
        patch.CountryAlpha2Code.SpecifiedValue.Should().Be("NL");
        patch.Port.IsSpecified.Should().BeTrue();
        patch.Port.SpecifiedValue.Should().BeNull();
        patch.Host.SpecifiedValue.Should().BeNull();
        patch.Path.SpecifiedValue.Should().Be("/vless");
        patch.Security.SpecifiedValue.Should().Be(HostSecurity.TLS);
        patch.ALPN.SpecifiedValue.Should().Be(ALPN.H1 | ALPN.H2);
        HostWireValues.ToFingerprintName(patch.Fingerprint.SpecifiedValue).Should().Be("ios");
        patch.Enabled.SpecifiedValue.Should().BeFalse();
    }

    [Fact]
    public void HostWireValues_ConvertsAlpnBitmask()
    {
        var parsed = HostWireValues.ParseAlpn(["http/1.1", "h3"]);

        parsed.Should().Be(ALPN.H1 | ALPN.H3);
        HostWireValues.ToAlpnNames(parsed).Should().Equal("http/1.1", "h3");
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("chrome", "chrome")]
    [InlineData("ios", "ios")]
    [InlineData("360", "360")]
    public void HostWireValues_RoundTripsFingerprint(string value, string expected)
    {
        var parsed = HostWireValues.ParseFingerprint(value);

        HostWireValues.ToFingerprintName(parsed).Should().Be(expected);
    }
}
