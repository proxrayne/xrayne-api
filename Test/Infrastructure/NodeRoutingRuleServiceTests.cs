using Contracts.Enums;
using Contracts.Models;
using Data.Contracts;
using Data.Entities;
using Data.Implementations;
using Infrastructure.Dto;
using Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using RemoteNode.Models;
using RemoteNode.Services;
using Xray.Config.Models;

namespace Test.Infrastructure;

public sealed class NodeRoutingRuleServiceTests
{
    private static readonly Guid AdminId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private readonly INodeRepository nodes;
    private readonly IRoutingRuleRepository routingRules;
    private readonly INodeSecretService secrets;
    private readonly IRemoteNodeApiClient remoteClient;
    private readonly IRemoteNodeApiClientFactory apiClientFactory;
    private readonly RemoteNodeCoreStateStore coreStateStore;
    private readonly NodeEntity node;
    private readonly NodeRoutingRuleService service;

    public NodeRoutingRuleServiceTests()
    {
        nodes = Substitute.For<INodeRepository>();
        routingRules = Substitute.For<IRoutingRuleRepository>();
        secrets = Substitute.For<INodeSecretService>();
        remoteClient = Substitute.For<IRemoteNodeApiClient>();
        apiClientFactory = Substitute.For<IRemoteNodeApiClientFactory>();
        coreStateStore = new RemoteNodeCoreStateStore(new MemoryCache(new MemoryCacheOptions()));
        node = CreateNode();

        nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        secrets.UnprotectApiKey(node.EncryptedApiKey).Returns("api-key");
        apiClientFactory.Create(Arg.Any<RemoteNodeEndpoint>()).Returns(remoteClient);
        routingRules.AddAsync(
                Arg.Any<Guid>(),
                Arg.Any<long>(),
                Arg.Any<RoutingRuleEntity>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var rule = call.Arg<RoutingRuleEntity>();
                rule.Id = rule.Id == 0 ? 10 : rule.Id;
                return rule;
            });
        routingRules.UpdateAsync(Arg.Any<RoutingRuleEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<RoutingRuleEntity>());

        service = new NodeRoutingRuleService(
            nodes,
            routingRules,
            secrets,
            apiClientFactory,
            coreStateStore);
    }

    [Fact]
    public async Task CreateAsync_AppendsManualRuleAfterExistingRules()
    {
        routingRules.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([CreateRule(id: 1, position: 0), CreateRule(id: 2, position: 10)]);

        var result = await service.CreateAsync(
            AdminId,
            node.Id,
            "Manual",
            """{"type":"field","outboundTag":"direct"}""",
            false,
            CancellationToken.None);

        result.Tag.Should().Be("Manual");
        result.Enabled.Should().BeFalse();
        result.Position.Should().Be(20);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsReadonly_WhenRuleIsReadonly()
    {
        var rule = CreateRule(readOnly: true);
        routingRules.GetByNodeAndIdAsync(node.Id, rule.Id, Arg.Any<CancellationToken>())
            .Returns(rule);

        var act = () => service.UpdateAsync(
            node.Id,
            rule.Id,
            "Updated",
            """{"type":"field","outboundTag":"direct"}""",
            true,
            CancellationToken.None);

        await act.Should().ThrowAsync<NodeRoutingRuleReadonlyException>();
    }

    [Fact]
    public async Task UpdateOrderAsync_ReordersOnlyManualRulesAfterReadonlyRules()
    {
        var readonlyRule = CreateRule(id: 1, position: 0, readOnly: true);
        var firstManual = CreateRule(id: 2, position: 10);
        var secondManual = CreateRule(id: 3, position: 20);
        routingRules.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([readonlyRule, firstManual, secondManual]);

        var result = await service.UpdateOrderAsync(node.Id, [3, 2], CancellationToken.None);

        result.OrderBy(rule => rule.Position).Select(rule => rule.Id).Should().Equal(1, 3, 2);
        readonlyRule.Position.Should().Be(0);
        secondManual.Position.Should().Be(10);
        firstManual.Position.Should().Be(20);
    }

    [Fact]
    public async Task UpdateEnabledAsync_SyncsEnabledRules_WhenCoreIsRunning()
    {
        var rule = CreateRule(enabled: false);
        routingRules.GetByNodeAndIdAsync(node.Id, rule.Id, Arg.Any<CancellationToken>())
            .Returns(rule);
        routingRules.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([rule]);
        coreStateStore.Set(new RemoteNodeCoreState(
            node.Id,
            true,
            true,
            "1.0.0",
            CoreStatus.Started,
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(1)));

        var result = await service.UpdateEnabledAsync(node.Id, rule.Id, true, CancellationToken.None);

        result.Enabled.Should().BeTrue();
        await remoteClient.Received(1).SyncRoutingRulesAsync(
            Arg.Is<SyncRoutingRulesRequest>(request => request.RoutingRules.Any(rule => rule.RoutingRule.OutboundTag == "direct")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncReadonlyFromTemplateAsync_CreatesReadonlyRulesInTemplateOrder()
    {
        routingRules.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([], [CreateRule(id: 10, position: 0, readOnly: true), CreateRule(id: 11, position: 10, readOnly: true)]);
        var nextId = 100L;
        routingRules.AddAsync(
                AdminId,
                node.Id,
                Arg.Any<RoutingRuleEntity>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var rule = call.Arg<RoutingRuleEntity>();
                rule.Id = nextId++;

                return rule;
            });
        var template = new XrayConfig
        {
            Routing = new RoutingConfig
            {
                Rules =
                [
                    new RoutingRule { OutboundTag = "first" },
                    new RoutingRule { OutboundTag = "second" }
                ]
            }
        };

        await service.SyncReadonlyFromTemplateAsync(AdminId, node, template, CancellationToken.None);

        await routingRules.Received(1).AddAsync(
            AdminId,
            node.Id,
            Arg.Is<RoutingRuleEntity>(rule => rule.ReadOnly && rule.Position == 0),
            Arg.Any<CancellationToken>());
        await routingRules.Received(1).AddAsync(
            AdminId,
            node.Id,
            Arg.Is<RoutingRuleEntity>(rule => rule.ReadOnly && rule.Position == 10),
            Arg.Any<CancellationToken>());
        await routingRules.Received(1).UpdateAsync(
            Arg.Is<RoutingRuleEntity>(rule => rule.Id == 100 && rule.Tag == "Rule #100"),
            Arg.Any<CancellationToken>());
        await routingRules.Received(1).UpdateAsync(
            Arg.Is<RoutingRuleEntity>(rule => rule.Id == 101 && rule.Tag == "Rule #101"),
            Arg.Any<CancellationToken>());
    }

    private static NodeEntity CreateNode()
    {
        return new NodeEntity
        {
            Id = 1,
            Name = "Node",
            Address = "node.example.com",
            Port = 22,
            ApiPort = 8443,
            SSHUsername = "root",
            WorkingDirectory = "/opt/xrayne",
            EncryptedApiKey = "encrypted",
            ApiKeyFingerprint = "fingerprint",
            LastStatusChange = DateTime.UtcNow,
            InstallationMessage = "Connected."
        };
    }

    private static RoutingRuleEntity CreateRule(
        long id = 2,
        int position = 0,
        bool enabled = true,
        bool readOnly = false,
        string outboundTag = "direct")
    {
        return new RoutingRuleEntity
        {
            Id = id,
            Tag = $"Rule {id}",
            Enabled = enabled,
            ReadOnly = readOnly,
            Position = position,
            Config = new RoutingRule { OutboundTag = outboundTag }
        };
    }
}
