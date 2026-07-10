using System.Text.Json.Nodes;
using Contracts.Utilities;
using Infrastructure.Services;
using Data.Entities;
using Xray.Config.Enums;
using Xray.Config.Models;

namespace Test.Infrastructure;

public sealed class NodeCoreConfigBuilderTests
{
    [Fact]
    public void Build_AddsManagedSectionsAndPreservesTemplateSections()
    {
        var node = new NodeEntity
        {
            Name = "Node",
            Address = "node.example.com",
            Port = 22,
            ApiPort = 8443,
            SSHUsername = "root",
            WorkingDirectory = "/opt/xrayne",
            EncryptedApiKey = "encrypted",
            ApiKeyFingerprint = "fingerprint",
            ConfigTemplate = DeserializeConfig("""{"log":{"loglevel":"warning"},"stats":{}}"""),
            Inbounds =
            [
                new InboundEntity
                {
                    Id = 1,
                    Enabled = true,
                    Config = new SocksInbound
                    {
                        Tag = "socks-in",
                        Listen = "0.0.0.0",
                        Port = new Port(10808),
                        Settings = new Inbound.SocksSettings
                        {
                            Auth = SocksAuth.NoAuth,
                            Udp = true
                        }
                    }
                },
                new InboundEntity
                {
                    Id = 2,
                    Enabled = false,
                    ReadOnly = true,
                    Config = new SocksInbound
                    {
                        Tag = "disabled-in",
                        Listen = "0.0.0.0",
                        Port = new Port(10809),
                        Settings = new Inbound.SocksSettings()
                    }
                }
            ],
            Outbounds =
            [
                new OutboundEntity
                {
                    Id = 1,
                    Enabled = true,
                    CreatedAt = DateTimeOffset.Parse("2026-01-02T00:00:00Z"),
                    Config = new FreedomOutbound { Tag = "second" }
                },
                new OutboundEntity
                {
                    Id = 2,
                    Enabled = true,
                    CreatedAt = DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
                    Config = new FreedomOutbound { Tag = "first" }
                },
                new OutboundEntity
                {
                    Id = 3,
                    Enabled = false,
                    CreatedAt = DateTimeOffset.Parse("2026-01-03T00:00:00Z"),
                    Config = new FreedomOutbound { Tag = "disabled" }
                },
                new OutboundEntity
                {
                    Id = 4,
                    Enabled = true,
                    ReadOnly = true,
                    CreatedAt = DateTimeOffset.Parse("2026-01-04T00:00:00Z"),
                    Config = new FreedomOutbound { Tag = "readonly" }
                },
                new OutboundEntity
                {
                    Id = 5,
                    Enabled = false,
                    ReadOnly = true,
                    CreatedAt = DateTimeOffset.Parse("2026-01-05T00:00:00Z"),
                    Config = new FreedomOutbound { Tag = "disabled-readonly" }
                }
            ],
            RoutingRules =
            [
                new RoutingRuleEntity
                {
                    Id = 1,
                    Tag = "disabled-rule",
                    Position = 1,
                    Enabled = false,
                    Config = new RoutingRule { OutboundTag = "disabled" }
                },
                new RoutingRuleEntity
                {
                    Id = 2,
                    Tag = "enabled-rule",
                    Position = 2,
                    Enabled = true,
                    Config = new RoutingRule { OutboundTag = "first" }
                }
            ]
        };
        var builder = new NodeCoreConfigBuilder();

        var request = builder.Build(node);
        var config = ToJsonObject(request.ConfigTemplate);

        config["stats"].Should().NotBeNull();
        config.ContainsKey("inbounds").Should().BeFalse();
        config.ContainsKey("outbounds").Should().BeFalse();
        request.Inbounds.Should().ContainSingle(item => item.Id == 1 && item.Position == 0);
        request.Outbounds.Should().HaveCount(3);
        request.Outbounds[0].Outbound.Tag.Should().Be("first");
        request.Outbounds[0].Position.Should().Be(0);
        request.Outbounds[1].Outbound.Tag.Should().Be("second");
        request.Outbounds[1].Position.Should().Be(1);
        request.Outbounds[2].Outbound.Tag.Should().Be("readonly");
        request.RoutingRules.Should().ContainSingle(item => item.Id == 2 && item.Position == 2);
    }

    [Fact]
    public void Build_ReplacesTemplateManagedSectionsWithEmptyManagedSections()
    {
        var node = new NodeEntity
        {
            Name = "Node",
            Address = "node.example.com",
            Port = 22,
            ApiPort = 8443,
            SSHUsername = "root",
            WorkingDirectory = "/opt/xrayne",
            EncryptedApiKey = "encrypted",
            ApiKeyFingerprint = "fingerprint",
            ConfigTemplate = new XrayConfig
            {
                Inbounds =
                [
                    new SocksInbound
                    {
                        Tag = "template-in",
                        Listen = "0.0.0.0",
                        Port = new Port(10808),
                        Settings = new Inbound.SocksSettings { Auth = SocksAuth.NoAuth }
                    }
                ],
                Outbounds = [new FreedomOutbound { Tag = "template-out" }],
                Routing = new RoutingConfig
                {
                    Rules = [new RoutingRule { OutboundTag = "template-out" }]
                }
            }
        };
        var builder = new NodeCoreConfigBuilder();

        var request = builder.Build(node);
        var config = ToJsonObject(request.ConfigTemplate);

        config.ContainsKey("inbounds").Should().BeFalse();
        config.ContainsKey("outbounds").Should().BeFalse();
        config["routing"]!.AsObject().ContainsKey("rules").Should().BeFalse();
        request.Inbounds.Should().BeEmpty();
        request.Outbounds.Should().BeEmpty();
        request.RoutingRules.Should().BeEmpty();
    }

    private static XrayConfig DeserializeConfig(string json)
    {
        return XrayJsonSerializer.DeserializeRequired<XrayConfig>(json, "Config cannot be empty.");
    }

    private static JsonObject ToJsonObject(XrayConfig config)
    {
        var json = XrayJsonSerializer.Serialize(config);
        return JsonNode.Parse(json)!.AsObject();
    }
}
