using System.Text.Json;
using Api.Controllers;
using Api.Mapping;
using AutoMapper;
using Data.Entities;
using Infrastructure.Services;
using Xray.Config.Models;

namespace Test.Controllers;

/// <summary>
/// Tests node outbound API response contracts.
/// </summary>
public sealed class NodeOutboundsControllerTests
{
    private readonly INodeOutboundService outbounds;
    private readonly NodeOutboundsController controller;

    public NodeOutboundsControllerTests()
    {
        outbounds = Substitute.For<INodeOutboundService>();
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<NodeMappingProfile>()).CreateMapper();
        controller = new NodeOutboundsController(outbounds, mapper);
    }

    [Fact]
    public async Task GetAll_ReturnsListItemsWithoutConfig()
    {
        var outbound = CreateOutbound();
        outbounds.GetByNodeIdAsync(1, Arg.Any<CancellationToken>())
            .Returns([outbound]);

        var result = await controller.GetAll(1, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Tag.Should().Be(outbound.Tag);
        var json = JsonSerializer.Serialize(result);
        json.Should().NotContain("Config");
    }

    [Fact]
    public async Task GetById_ReturnsOutboundWithConfig()
    {
        var outbound = CreateOutbound();
        outbounds.GetByNodeAndIdAsync(1, outbound.Id, Arg.Any<CancellationToken>())
            .Returns(outbound);

        var result = await controller.GetById(1, outbound.Id, CancellationToken.None);

        result.Config.Should().Contain("\"tag\"");
        result.Tag.Should().Be(outbound.Tag);
    }

    private static OutboundEntity CreateOutbound()
    {
        return new OutboundEntity
        {
            Id = 10,
            Enabled = true,
            ReadOnly = false,
            Config = new FreedomOutbound { Tag = "direct" }
        };
    }
}
