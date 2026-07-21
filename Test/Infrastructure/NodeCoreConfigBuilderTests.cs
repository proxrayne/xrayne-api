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
                    Position = 1,
                    Enabled = false,
                    Config = new RoutingRule { RuleTag = "disabled-rule", OutboundTag = "disabled" }
                },
                new RoutingRuleEntity
                {
                    Id = 2,
                    Position = 2,
                    Enabled = true,
                    Config = new RoutingRule { RuleTag = "enabled-rule", OutboundTag = "first" }
                }
            ]
        };
        var builder = new NodeCoreConfigBuilder();

        var result = builder.Build(node);
        var config = ToJsonObject(result);

        config["stats"].Should().NotBeNull();
        result.Inbounds.Should().ContainSingle(inbound => inbound.Tag == "socks-in");
        result.Outbounds.Should().HaveCount(3);
        result.Outbounds.Select(outbound => outbound.Tag)
            .Should()
            .Equal("first", "second", "readonly");
        result.Routing!.Rules.Should().ContainSingle(rule => rule.RuleTag == "enabled-rule");
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

        var result = builder.Build(node);
        var config = ToJsonObject(result);

        config.ContainsKey("inbounds").Should().BeTrue();
        config.ContainsKey("outbounds").Should().BeTrue();
        config["routing"]!.AsObject().ContainsKey("rules").Should().BeTrue();
        result.Inbounds.Should().BeEmpty();
        result.Outbounds.Should().BeEmpty();
        result.Routing!.Rules.Should().BeEmpty();
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
