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

        var config = ToJsonObject(builder.Build(node));

        config["stats"].Should().NotBeNull();
        config["inbounds"]!.AsArray().Should().HaveCount(1);
        config["outbounds"]!.AsArray().Should().HaveCount(3);
        config["outbounds"]![0]!["tag"]!.GetValue<string>().Should().Be("first");
        config["outbounds"]![1]!["tag"]!.GetValue<string>().Should().Be("second");
        config["outbounds"]![2]!["tag"]!.GetValue<string>().Should().Be("readonly");
        config["routing"]!["rules"]!.AsArray().Should().HaveCount(1);
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

        var config = ToJsonObject(builder.Build(node));

        config["inbounds"]!.AsArray().Should().BeEmpty();
        config["outbounds"]!.AsArray().Should().BeEmpty();
        config["routing"]!["rules"]!.AsArray().Should().BeEmpty();
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
