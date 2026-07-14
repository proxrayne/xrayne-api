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

public sealed class NodeOutboundServiceTests
{
    private static readonly Guid AdminId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private readonly INodeRepository nodes;
    private readonly IOutboundRepository outbounds;
    private readonly INodeSecretService secrets;
    private readonly IRemoteNodeApiClient remoteClient;
    private readonly IRemoteNodeApiClientFactory apiClientFactory;
    private readonly RemoteNodeCoreStateStore coreStateStore;
    private readonly NodeEntity node;
    private readonly NodeOutboundService service;

    public NodeOutboundServiceTests()
    {
        nodes = Substitute.For<INodeRepository>();
        outbounds = Substitute.For<IOutboundRepository>();
        secrets = Substitute.For<INodeSecretService>();
        remoteClient = Substitute.For<IRemoteNodeApiClient>();
        apiClientFactory = Substitute.For<IRemoteNodeApiClientFactory>();
        coreStateStore = new RemoteNodeCoreStateStore(new MemoryCache(new MemoryCacheOptions()));
        node = CreateNode();

        nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        secrets.UnprotectApiKey(node.EncryptedApiKey).Returns("api-key");
        apiClientFactory.Create(Arg.Any<RemoteNodeEndpoint>()).Returns(remoteClient);
        outbounds.AddAsync(
                Arg.Any<Guid>(),
                Arg.Any<long>(),
                Arg.Any<OutboundEntity>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var outbound = call.Arg<OutboundEntity>();
                outbound.Id = outbound.Id == 0 ? 10 : outbound.Id;
                return outbound;
            });
        outbounds.UpdateAsync(Arg.Any<OutboundEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<OutboundEntity>());

        service = new NodeOutboundService(
            nodes,
            outbounds,
            secrets,
            apiClientFactory,
            coreStateStore);
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenTagAlreadyExists()
    {
        outbounds.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([CreateOutbound(id: 1, tag: "direct")]);

        var act = () => service.CreateAsync(
            AdminId,
            node.Id,
            """{"tag":"direct","protocol":"freedom"}""",
            true,
            CancellationToken.None);

        await act.Should().ThrowAsync<NodeOutboundConflictException>()
            .WithMessage("Outbound tag 'direct' already exists on this node.");
    }

    [Fact]
    public async Task UpdateAsync_ThrowsReadonly_WhenOutboundIsReadonly()
    {
        var outbound = CreateOutbound(readOnly: true);
        outbounds.GetByNodeAndIdAsync(node.Id, outbound.Id, Arg.Any<CancellationToken>())
            .Returns(outbound);

        var act = () => service.UpdateAsync(
            node.Id,
            outbound.Id,
            """{"tag":"proxy","protocol":"freedom"}""",
            true,
            CancellationToken.None);

        await act.Should().ThrowAsync<NodeOutboundReadonlyException>();
    }

    [Fact]
    public async Task UpdateAsync_SendsRemoteUpdateWithOldTag_WhenTagChanges()
    {
        var outbound = CreateOutbound(tag: "direct");
        outbounds.GetByNodeAndIdAsync(node.Id, outbound.Id, Arg.Any<CancellationToken>())
            .Returns(outbound);
        outbounds.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([outbound]);
        coreStateStore.Set(new RemoteNodeCoreState(
            node.Id,
            true,
            true,
            "1.0.0",
            CoreStatus.Started,
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(1)));

        var result = await service.UpdateAsync(
            node.Id,
            outbound.Id,
            """{"tag":"proxy","protocol":"freedom"}""",
            true,
            CancellationToken.None);

        result.Tag.Should().Be("proxy");
        await remoteClient.Received(1).UpdateOutboundAsync(
            "direct",
            Arg.Is<SyncOutboundRequest>(request => request.Outbound.Tag == "proxy"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateEnabledAsync_AddsRemoteOutbound_WhenCoreIsRunning()
    {
        var outbound = CreateOutbound(enabled: false);
        outbounds.GetByNodeAndIdAsync(node.Id, outbound.Id, Arg.Any<CancellationToken>())
            .Returns(outbound);
        outbounds.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([outbound]);
        coreStateStore.Set(new RemoteNodeCoreState(
            node.Id,
            true,
            true,
            "1.0.0",
            CoreStatus.Started,
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(1)));

        var result = await service.UpdateEnabledAsync(node.Id, outbound.Id, true, CancellationToken.None);

        result.Enabled.Should().BeTrue();
        await remoteClient.Received(1).AddOutboundAsync(
            Arg.Is<SyncOutboundRequest>(request => request.Outbound.Tag == "direct"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_DoesNotCallRemote_WhenCoreIsStopped()
    {
        var outbound = CreateOutbound(enabled: true);
        outbounds.GetByNodeAndIdAsync(node.Id, outbound.Id, Arg.Any<CancellationToken>())
            .Returns(outbound);
        outbounds.DeleteAsync(outbound.Id, Arg.Any<CancellationToken>()).Returns(true);

        await service.DeleteAsync(node.Id, outbound.Id, CancellationToken.None);

        await remoteClient.DidNotReceive()
            .DeleteOutboundAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncReadonlyFromTemplateAsync_SkipsReadonly_WhenTagConflicts()
    {
        var manual = CreateOutbound(id: 1, tag: "direct", enabled: true);
        outbounds.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([manual], [manual]);
        var template = new XrayConfig
        {
            Outbounds = [new FreedomOutbound { Tag = "direct" }]
        };

        await service.SyncReadonlyFromTemplateAsync(AdminId, node, template, CancellationToken.None);

        await outbounds.DidNotReceive().AddAsync(
            Arg.Any<Guid>(),
            Arg.Any<long>(),
            Arg.Any<OutboundEntity>(),
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

    private static OutboundEntity CreateOutbound(
        int id = 2,
        string tag = "direct",
        bool enabled = true,
        bool readOnly = false)
    {
        return new OutboundEntity
        {
            Id = id,
            Enabled = enabled,
            ReadOnly = readOnly,
            Config = new FreedomOutbound { Tag = tag }
        };
    }
}
