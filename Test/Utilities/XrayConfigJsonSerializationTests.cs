using System.Text.Json.Nodes;
using Contracts.Utilities;
using Xray.Config.Enums;
using Xray.Config.Models;

namespace Test.Utilities;

/// <summary>
/// Tests stable Xray configuration JSON serialization contracts.
/// </summary>
public sealed class XrayConfigJsonSerializationTests
{
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
}
