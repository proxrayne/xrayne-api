using System.Text.Json.Nodes;
using Contracts.Enums;
using Contracts.Utilities;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Entities;
using Xray.Config.Enums;
using Xray.Config.Models;

namespace Test.Data;

/// <summary>
/// Tests JSON persistence mapping for Xray configuration models.
/// </summary>
public sealed class XrayConfigJsonMappingTests
{
    [Fact]
    public void Model_KeepsXrayConfigColumnsAsJsonbWithoutValueConverters()
    {
        using var context = CreateNpgsqlModelContext();

        context.Model.FindEntityType(typeof(InboundEntity))!
            .FindProperty(nameof(InboundEntity.Config))!
            .GetColumnType()
            .Should()
            .Be("jsonb");
        context.Model.FindEntityType(typeof(InboundEntity))!
            .FindProperty(nameof(InboundEntity.Config))!
            .GetValueConverter()
            .Should()
            .BeNull();
        context.Model.FindEntityType(typeof(NodeEntity))!
            .FindProperty(nameof(NodeEntity.ConfigTemplate))!
            .GetColumnType()
            .Should()
            .Be("jsonb");
        context.Model.FindEntityType(typeof(NodeEntity))!
            .FindProperty(nameof(NodeEntity.ConfigTemplate))!
            .GetValueConverter()
            .Should()
            .BeNull();
    }

    [Fact]
    public void Model_MapsPostgresEnumsForRuntimeQueries()
    {
        using var context = CreateNpgsqlModelContext();

        var sql = context.GeoResources
            .Where(resource => resource.SourceType == GeoResourceSourceType.AutoUpdate)
            .ToQueryString();

        sql.Should().Contain("auto_update");
        sql.Should().NotContain("= 1");
    }

    [Fact]
    public void JsonSerializer_RoundTripsProtocolSpecificInbound()
    {
        var inbound = XrayJsonSerializer.Deserialize<Inbound>(
            """
            {
              "tag": "vless-in",
              "port": 443,
              "protocol": "vless",
              "settings": {
                "clients": [],
                "decryption": "none"
              },
              "streamSettings": {
                "network": "tcp",
                "security": "none"
              }
            }
            """);

        inbound.Should().NotBeNull();
        inbound!.Tag.Should().Be("vless-in");
        inbound.Protocol.Should().Be(Protocol.Vless);

        var json = XrayJsonSerializer.Serialize(inbound);
        JsonNode.Parse(json)!["protocol"]!.GetValue<string>().Should().Be("vless");

        XrayJsonSerializer.Deserialize<Inbound>(json)!
            .Protocol
            .Should()
            .Be(Protocol.Vless);
    }

    [Fact]
    public void JsonSerializer_RoundTripsConfigTemplateWithProtocolSpecificInbound()
    {
        var template = XrayJsonSerializer.Deserialize<XrayConfig>(
            """
            {
              "inbounds": [
                {
                  "tag": "template-in",
                  "port": 8443,
                  "protocol": "vless",
                  "settings": {
                    "clients": [],
                    "decryption": "none"
                  },
                  "streamSettings": {
                    "network": "tcp",
                    "security": "none"
                  }
                }
              ]
            }
            """);

        template.Should().NotBeNull();

        var json = XrayJsonSerializer.Serialize(template);
        JsonNode.Parse(json)!["inbounds"]![0]!["tag"]!.GetValue<string>().Should().Be("template-in");

        var restoredTemplate = XrayJsonSerializer.Deserialize<XrayConfig>(json)!;
        restoredTemplate.Inbounds.Should().ContainSingle(inbound => inbound.Tag == "template-in");
    }

    private static AppDbContext CreateNpgsqlModelContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseXRayneNpgsql("Host=localhost;Database=xrayne;Username=xrayne;Password=xrayne")
            .Options;

        return new AppDbContext(options);
    }
}
