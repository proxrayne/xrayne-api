using System.Text.Json.Nodes;
using Contracts.Enums;
using Infrastructure.Services;
using Repositories.Entities;
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
            ConfigTemplate = XrayConfig.FromJson("""{"log":{"loglevel":"warning"},"stats":{}}"""),
            Inbounds =
            [
                new InboundEntity
                {
                    Id = 1,
                    DisplayName = "Enabled",
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
                    DisplayName = "Disabled",
                    Enabled = false,
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
                    Position = 2,
                    Enabled = true,
                    Config = new FreedomOutbound { Tag = "second" }
                },
                new OutboundEntity
                {
                    Id = 2,
                    Position = 1,
                    Enabled = true,
                    Config = new FreedomOutbound { Tag = "first" }
                },
                new OutboundEntity
                {
                    Id = 3,
                    Position = 3,
                    Enabled = false,
                    Config = new FreedomOutbound { Tag = "disabled" }
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

        var config = JsonNode.Parse(builder.Build(node).ToJson())!.AsObject();

        config["stats"].Should().NotBeNull();
        config["inbounds"]!.AsArray().Should().HaveCount(1);
        config["outbounds"]!.AsArray().Should().HaveCount(2);
        config["outbounds"]![0]!["tag"]!.GetValue<string>().Should().Be("first");
        config["outbounds"]![1]!["tag"]!.GetValue<string>().Should().Be("second");
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

        var config = JsonNode.Parse(builder.Build(node).ToJson())!.AsObject();

        config["inbounds"]!.AsArray().Should().BeEmpty();
        config["outbounds"]!.AsArray().Should().BeEmpty();
        config["routing"]!["rules"]!.AsArray().Should().BeEmpty();
    }
}
