using System.Text.Json;
using Api.Controllers;
using Api.Mapping;
using Api.Responses;
using AutoMapper;
using Data.Entities;
using Infrastructure.Services;
using Xray.Config.Enums;
using Xray.Config.Models;

namespace Test.Controllers;

/// <summary>
/// Tests node inbound API response contracts.
/// </summary>
public sealed class NodeInboundsControllerTests
{
    private readonly INodeInboundService inbounds;
    private readonly NodeInboundsController controller;

    public NodeInboundsControllerTests()
    {
        inbounds = Substitute.For<INodeInboundService>();
        var mapper = MapperTestFactory.CreateConfiguration(cfg => cfg.AddProfile<NodeMappingProfile>()).CreateMapper();
        controller = new NodeInboundsController(inbounds, mapper);
    }

    [Fact]
    public async Task GetAll_ReturnsListItemsWithoutConfig()
    {
        var inbound = CreateInbound();
        inbounds.GetByNodeIdAsync(1, Arg.Any<CancellationToken>())
            .Returns([inbound]);

        var result = await controller.GetAll(1, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Tag.Should().Be(inbound.Tag);
        var json = JsonSerializer.Serialize(result);
        json.Should().NotContain("Config");
    }

    [Fact]
    public async Task GetById_ReturnsInboundWithConfig()
    {
        var inbound = CreateInbound();
        inbounds.GetByNodeAndIdAsync(1, inbound.Id, Arg.Any<CancellationToken>())
            .Returns(inbound);

        var result = await controller.GetById(1, inbound.Id, CancellationToken.None);

        result.Config.Should().Contain("\"tag\"");
        result.Id.Should().Be(inbound.Id);
        result.Tag.Should().Be(inbound.Tag);
    }

    private static InboundEntity CreateInbound()
    {
        return new InboundEntity
        {
            Id = 10,
            Enabled = true,
            ReadOnly = false,
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
        };
    }
}
