using System.Security.Claims;
using Api.Controllers;
using Api.Mapping;
using Api.Requests;
using AutoMapper;
using Data.Entities;
using Infrastructure.Dto;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xray.Config.Models;

namespace Test.Controllers;

/// <summary>
/// Tests node routing rule API response contracts and service delegation.
/// </summary>
public sealed class NodeRoutingRulesControllerTests
{
    private static readonly Guid TestAdminId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private readonly INodeRoutingRuleService routingRules;
    private readonly NodeRoutingRulesController controller;

    public NodeRoutingRulesControllerTests()
    {
        routingRules = Substitute.For<INodeRoutingRuleService>();
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<NodeMappingProfile>()).CreateMapper();
        controller = new NodeRoutingRulesController(routingRules, mapper)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() },
        };
    }

    [Fact]
    public async Task GetAll_ReturnsListItemsWithoutConfig()
    {
        var routingRule = CreateRoutingRule();
        routingRules.GetByNodeIdAsync(1, Arg.Any<CancellationToken>())
            .Returns([routingRule]);

        var result = await controller.GetAll(1, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Tag.Should().Be(routingRule.RuleTag);
        result[0].InboundTags.Should().ContainSingle("socks-in");
        result[0].OutboundTag.Should().Be("direct");
        result[0].Config.Should().Contain("\"outboundTag\"");
    }

    [Fact]
    public async Task GetById_ReturnsRoutingRuleWithConfig()
    {
        var routingRule = CreateRoutingRule();
        routingRules.GetByNodeAndIdAsync(1, routingRule.Id, Arg.Any<CancellationToken>())
            .Returns(routingRule);

        var result = await controller.GetById(1, routingRule.Id, CancellationToken.None);

        result.Config.Should().Contain("\"outboundTag\"");
        result.Id.Should().Be(routingRule.Id);
        result.Tag.Should().Be(routingRule.RuleTag);
    }

    [Fact]
    public async Task Create_DelegatesToServiceAndReturnsCreated()
    {
        var request = new CreateNodeRoutingRuleRequest
        {
            Config = """{"type":"field","outboundTag":"direct"}""",
            Enabled = false
        };
        var routingRule = CreateRoutingRule(tag: "manual-rule", enabled: request.Enabled);
        routingRules.CreateAsync(
                TestAdminId,
                1,
                request.Config,
                request.Enabled,
                Arg.Any<CancellationToken>())
            .Returns(routingRule);

        var result = await controller.Create(1, request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.Location.Should().Be($"/api/nodes/1/routing-rules/{routingRule.Id}");
        created.Value.Should().BeEquivalentTo(
            new
            {
                routingRule.Id,
                Tag = routingRule.RuleTag,
                routingRule.Enabled,
                routingRule.ReadOnly,
                routingRule.Position
            });
    }

    [Fact]
    public async Task Update_DelegatesToService()
    {
        var request = new UpdateNodeRoutingRuleRequest
        {
            Config = """{"type":"field","outboundTag":"block"}""",
            Enabled = true
        };
        routingRules.UpdateAsync(1, 10, request.Config, request.Enabled, Arg.Any<CancellationToken>())
            .Returns(CreateRoutingRule(id: 10, tag: "updated-rule", enabled: request.Enabled, outboundTag: "block"));

        var result = await controller.Update(1, 10, request, CancellationToken.None);

        result.Tag.Should().Be("updated-rule");
        result.Enabled.Should().BeTrue();
        result.OutboundTag.Should().Be("block");
    }

    [Fact]
    public async Task UpdateEnabled_DelegatesToService()
    {
        routingRules.UpdateEnabledAsync(1, 10, false, Arg.Any<CancellationToken>())
            .Returns(CreateRoutingRule(id: 10, enabled: false));

        var result = await controller.UpdateEnabled(
            1,
            10,
            new UpdateNodeRoutingRuleEnabledRequest { Enabled = false },
            CancellationToken.None);

        result.Enabled.Should().BeFalse();
    }

    [Fact]
    public async Task Save_DelegatesToServiceAndReturnsListItems()
    {
        var request = new SaveNodeRoutingRulesRequest
        {
            ManualRules =
            [
                new SaveNodeRoutingRuleManualRequest
                {
                    Id = 10,
                    Config = """{"type":"field","ruleTag":"manual-rule","outboundTag":"direct"}""",
                    Enabled = true
                }
            ],
            ReadonlyRules =
            [
                new SaveNodeRoutingRuleReadonlyRequest { Id = 1, Enabled = false }
            ]
        };
        routingRules.SaveAsync(
                TestAdminId,
                1,
                Arg.Is<IReadOnlyCollection<NodeRoutingRuleManualSaveItem>>(rules =>
                    rules.Single().Id == 10 && rules.Single().Enabled),
                Arg.Is<IReadOnlyCollection<NodeRoutingRuleReadonlySaveItem>>(rules =>
                    rules.Single().Id == 1 && !rules.Single().Enabled),
                Arg.Any<CancellationToken>())
            .Returns([
                CreateRoutingRule(id: 1, tag: "readonly-rule", enabled: false, readOnly: true, position: 0),
                CreateRoutingRule(id: 10, tag: "manual-rule", enabled: true, position: 10)
            ]);

        var result = await controller.Save(1, request, CancellationToken.None);

        result.Select(rule => rule.Id).Should().Equal(1, 10);
        result[0].ReadOnly.Should().BeTrue();
        result[0].Enabled.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateOrder_DelegatesToService()
    {
        routingRules.UpdateOrderAsync(1, Arg.Is<IReadOnlyList<long>>(ids => ids.SequenceEqual(new long[] { 11, 10 })), Arg.Any<CancellationToken>())
            .Returns([
                CreateRoutingRule(id: 11, tag: "rule-11", position: 0),
                CreateRoutingRule(id: 10, tag: "rule-10", position: 10)
            ]);

        var result = await controller.UpdateOrder(
            1,
            new UpdateNodeRoutingRuleOrderRequest { RuleIds = [11, 10] },
            CancellationToken.None);

        result.Select(rule => rule.Id).Should().Equal(11, 10);
    }

    [Fact]
    public async Task Delete_DelegatesToServiceAndReturnsNoContent()
    {
        var result = await controller.Delete(1, 10, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        await routingRules.Received(1).DeleteAsync(1, 10, Arg.Any<CancellationToken>());
    }

    private static RoutingRuleEntity CreateRoutingRule(
        long id = 10,
        string tag = "Rule",
        bool enabled = true,
        bool readOnly = false,
        int position = 0,
        string outboundTag = "direct")
    {
        return new RoutingRuleEntity
        {
            Id = id,
            Enabled = enabled,
            ReadOnly = readOnly,
            Position = position,
            Config = new RoutingRule
            {
                RuleTag = tag,
                InboundTag = ["socks-in"],
                OutboundTag = outboundTag
            }
        };
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, TestAdminId.ToString())],
            "Test"));

        return context;
    }
}
