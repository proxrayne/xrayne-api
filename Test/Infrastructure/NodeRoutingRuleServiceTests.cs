using System.Text.RegularExpressions;
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
            """{"type":"field","outboundTag":"direct"}""",
            false,
            CancellationToken.None);

        result.Config.RuleTag.Should().NotBeNullOrWhiteSpace();
        IsShortGuid(result.Config.RuleTag).Should().BeTrue();
        result.Enabled.Should().BeFalse();
        result.Position.Should().Be(20);
    }

    [Fact]
    public async Task CreateAsync_PreservesProvidedRuleTag()
    {
        routingRules.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await service.CreateAsync(
            AdminId,
            node.Id,
            """{"type":"field","ruleTag":"  manual-rule  ","outboundTag":"direct"}""",
            true,
            CancellationToken.None);

        result.Config.RuleTag.Should().Be("manual-rule");
    }

    [Fact]
    public async Task UpdateAsync_GeneratesRuleTag_WhenMissing()
    {
        var rule = CreateRule();
        routingRules.GetByNodeAndIdAsync(node.Id, rule.Id, Arg.Any<CancellationToken>())
            .Returns(rule);
        routingRules.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([rule]);

        var result = await service.UpdateAsync(
            node.Id,
            rule.Id,
            """{"type":"field","outboundTag":"block"}""",
            true,
            CancellationToken.None);

        result.Config.RuleTag.Should().NotBeNullOrWhiteSpace();
        IsShortGuid(result.Config.RuleTag).Should().BeTrue();
        result.Config.OutboundTag.Should().Be("block");
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
            Arg.Is<SyncRoutingRulesRequest>(request => request.RoutingRules.Any(rule => rule.OutboundTag == "direct")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAsync_SavesSnapshotAndSyncsRemoteOnce_WhenCoreIsRunning()
    {
        var current = new List<RoutingRuleEntity>
        {
            CreateRule(id: 1, position: 0, enabled: true, readOnly: true),
            CreateRule(id: 2, position: 10),
            CreateRule(id: 3, position: 20, outboundTag: "old")
        };
        routingRules.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns(_ => current.OrderBy(rule => rule.Position).ThenBy(rule => rule.Id).ToList());
        routingRules.SaveChangesAsync(
                AdminId,
                node.Id,
                Arg.Any<IReadOnlyCollection<RoutingRuleEntity>>(),
                Arg.Any<IReadOnlyCollection<RoutingRuleEntity>>(),
                Arg.Any<IReadOnlyCollection<RoutingRuleEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var rulesToCreate = call.ArgAt<IReadOnlyCollection<RoutingRuleEntity>>(2);
                var rulesToDelete = call.ArgAt<IReadOnlyCollection<RoutingRuleEntity>>(4);

                current.RemoveAll(rule => rulesToDelete.Any(deleted => deleted.Id == rule.Id));
                foreach (var rule in rulesToCreate)
                {
                    rule.Id = 100;
                    current.Add(rule);
                }

                return current.OrderBy(rule => rule.Position).ThenBy(rule => rule.Id).ToList();
            });
        coreStateStore.Set(new RemoteNodeCoreState(
            node.Id,
            true,
            true,
            "1.0.0",
            CoreStatus.Started,
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(1)));

        var result = await service.SaveAsync(
            AdminId,
            node.Id,
            [
                new NodeRoutingRuleManualSaveItem(
                    3,
                    """{"type":"field","ruleTag":"renamed-rule","outboundTag":"proxy"}""",
                    true),
                new NodeRoutingRuleManualSaveItem(
                    null,
                    """{"type":"field","ruleTag":"new-rule","outboundTag":"block"}""",
                    false)
            ],
            [
                new NodeRoutingRuleReadonlySaveItem(1, false)
            ],
            CancellationToken.None);

        result.OrderBy(rule => rule.Position).Select(rule => rule.Id).Should().Equal(1, 3, 100);
        current.Single(rule => rule.Id == 1).Enabled.Should().BeFalse();
        current.Single(rule => rule.Id == 3).Config.RuleTag.Should().Be("renamed-rule");
        current.Single(rule => rule.Id == 3).Config.OutboundTag.Should().Be("proxy");
        current.Single(rule => rule.Id == 3).Position.Should().Be(10);
        current.Single(rule => rule.Id == 100).Position.Should().Be(20);
        await routingRules.Received(1).SaveChangesAsync(
            AdminId,
            node.Id,
            Arg.Is<IReadOnlyCollection<RoutingRuleEntity>>(rules => rules.Count == 1),
            Arg.Is<IReadOnlyCollection<RoutingRuleEntity>>(rules =>
                rules.Select(rule => rule.Id).OrderBy(id => id).SequenceEqual(new long[] { 1, 3 })),
            Arg.Is<IReadOnlyCollection<RoutingRuleEntity>>(rules =>
                rules.Select(rule => rule.Id).SequenceEqual(new long[] { 2 })),
            Arg.Any<CancellationToken>());
        await routingRules.DidNotReceive().AddAsync(
            Arg.Any<Guid>(),
            Arg.Any<long>(),
            Arg.Any<RoutingRuleEntity>(),
            Arg.Any<CancellationToken>());
        await routingRules.DidNotReceive().UpdateAsync(
            Arg.Any<RoutingRuleEntity>(),
            Arg.Any<CancellationToken>());
        await routingRules.DidNotReceive().DeleteAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
        await remoteClient.Received(1).SyncRoutingRulesAsync(
            Arg.Is<SyncRoutingRulesRequest>(request =>
                request.RoutingRules.Select(rule => rule.RuleTag).SequenceEqual(new[] { "renamed-rule" })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAsync_ThrowsReadonly_WhenReadonlyRuleIsSubmittedAsManual()
    {
        routingRules.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([CreateRule(id: 1, readOnly: true)]);

        var act = () => service.SaveAsync(
            AdminId,
            node.Id,
            [
                new NodeRoutingRuleManualSaveItem(
                    1,
                    """{"type":"field","ruleTag":"readonly-rule","outboundTag":"direct"}""",
                    true)
            ],
            [],
            CancellationToken.None);

        await act.Should().ThrowAsync<NodeRoutingRuleReadonlyException>();
    }

    [Fact]
    public async Task SaveAsync_ThrowsNotFound_WhenManualRuleIdIsUnknown()
    {
        routingRules.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([]);

        var act = () => service.SaveAsync(
            AdminId,
            node.Id,
            [
                new NodeRoutingRuleManualSaveItem(
                    99,
                    """{"type":"field","ruleTag":"missing-rule","outboundTag":"direct"}""",
                    true)
            ],
            [],
            CancellationToken.None);

        await act.Should().ThrowAsync<NodeRoutingRuleNotFoundException>();
    }

    [Fact]
    public async Task SaveAsync_ThrowsValidation_WhenRuleTagsAreDuplicated()
    {
        routingRules.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([]);

        var act = () => service.SaveAsync(
            AdminId,
            node.Id,
            [
                new NodeRoutingRuleManualSaveItem(
                    null,
                    """{"type":"field","ruleTag":"same-rule","outboundTag":"direct"}""",
                    true),
                new NodeRoutingRuleManualSaveItem(
                    null,
                    """{"type":"field","ruleTag":"same-rule","outboundTag":"block"}""",
                    true)
            ],
            [],
            CancellationToken.None);

        await act.Should().ThrowAsync<NodeRoutingRuleValidationException>();
    }

    [Fact]
    public async Task SyncReadonlyFromTemplateAsync_CreatesReadonlyRulesInTemplateOrder()
    {
        routingRules.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([], [CreateRule(id: 10, position: 0, readOnly: true), CreateRule(id: 11, position: 10, readOnly: true)]);
        var nextId = 100L;
        var added = new List<RoutingRuleEntity>();
        routingRules.AddAsync(
                AdminId,
                node.Id,
                Arg.Any<RoutingRuleEntity>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var rule = call.Arg<RoutingRuleEntity>();
                added.Add(new RoutingRuleEntity
                {
                    Id = nextId,
                    Enabled = rule.Enabled,
                    ReadOnly = rule.ReadOnly,
                    Position = rule.Position,
                    Config = new RoutingRule
                    {
                        RuleTag = rule.Config.RuleTag,
                        OutboundTag = rule.Config.OutboundTag
                    }
                });
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

        added.Should().HaveCount(2);
        added[0].ReadOnly.Should().BeTrue();
        added[0].Position.Should().Be(0);
        IsShortGuid(added[0].Config.RuleTag).Should().BeTrue();
        added[1].ReadOnly.Should().BeTrue();
        added[1].Position.Should().Be(10);
        IsShortGuid(added[1].Config.RuleTag).Should().BeTrue();
    }

    private static bool IsShortGuid(string? value)
    {
        return value is not null && Regex.IsMatch(value, "^[a-f0-9]{8}$", RegexOptions.CultureInvariant);
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
            Enabled = enabled,
            ReadOnly = readOnly,
            Position = position,
            Config = new RoutingRule { RuleTag = $"rule-{id}", OutboundTag = outboundTag }
        };
    }
}
