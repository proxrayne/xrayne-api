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
using Xray.Config.Enums;
using Xray.Config.Models;

namespace Test.Infrastructure;

public sealed class NodeInboundServiceTests
{
    private static readonly Guid AdminId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private readonly INodeRepository nodes;
    private readonly IInboundRepository inbounds;
    private readonly INodeSecretService secrets;
    private readonly IRemoteNodeApiClient remoteClient;
    private readonly IRemoteNodeApiClientFactory apiClientFactory;
    private readonly RemoteNodeCoreStateStore coreStateStore;
    private readonly NodeEntity node;
    private readonly NodeInboundService service;

    public NodeInboundServiceTests()
    {
        nodes = Substitute.For<INodeRepository>();
        inbounds = Substitute.For<IInboundRepository>();
        secrets = Substitute.For<INodeSecretService>();
        remoteClient = Substitute.For<IRemoteNodeApiClient>();
        apiClientFactory = Substitute.For<IRemoteNodeApiClientFactory>();
        coreStateStore = new RemoteNodeCoreStateStore(new MemoryCache(new MemoryCacheOptions()));
        node = CreateNode();

        nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        secrets.UnprotectApiKey(node.EncryptedApiKey).Returns("api-key");
        apiClientFactory.Create(Arg.Any<RemoteNodeEndpoint>()).Returns(remoteClient);
        inbounds.UpdateAsync(Arg.Any<InboundEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<InboundEntity>());

        service = new NodeInboundService(
            nodes,
            inbounds,
            secrets,
            apiClientFactory,
            coreStateStore);
    }

    [Fact]
    public async Task UpdateAsync_SendsRemoteUpdateWithOldTag_WhenTagChanges()
    {
        var inbound = CreateInbound(tag: "socks-in", port: 10808);
        inbounds.GetByNodeAndIdAsync(node.Id, inbound.Id, Arg.Any<CancellationToken>())
            .Returns(inbound);
        inbounds.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([inbound]);
        SetCoreRunning();

        var result = await service.UpdateAsync(
            node.Id,
            inbound.Id,
            """{"tag":"socks-next","port":10809,"protocol":"socks","settings":{"auth":"noauth","udp":true}}""",
            true,
            CancellationToken.None);

        result.Tag.Should().Be("socks-next");
        await remoteClient.Received(1).UpdateInboundAsync(
            "socks-in",
            Arg.Is<SyncInboundRequest>(request => request.Inbound.Tag == "socks-next"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateEnabledAsync_AddsRemoteInbound_ByIdWhenTagChanged()
    {
        var inbound = CreateInbound(tag: "socks-next", port: 10809, enabled: false);
        inbounds.GetByNodeAndIdAsync(node.Id, inbound.Id, Arg.Any<CancellationToken>())
            .Returns(inbound);
        inbounds.GetByNodeIdAsync(node.Id, Arg.Any<CancellationToken>())
            .Returns([inbound]);
        SetCoreRunning();

        var result = await service.UpdateEnabledAsync(node.Id, inbound.Id, true, CancellationToken.None);

        result.Enabled.Should().BeTrue();
        await remoteClient.Received(1).AddInboundAsync(
            Arg.Is<SyncInboundRequest>(request => request.Inbound.Tag == "socks-next"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_DeletesByIdAndSkipsRemote_WhenCoreIsStopped()
    {
        var inbound = CreateInbound(tag: "socks-next", port: 10809, enabled: true);
        inbounds.GetByNodeAndIdAsync(node.Id, inbound.Id, Arg.Any<CancellationToken>())
            .Returns(inbound);
        inbounds.DeleteAsync(inbound.Id, Arg.Any<CancellationToken>()).Returns(true);

        await service.DeleteAsync(node.Id, inbound.Id, CancellationToken.None);

        await remoteClient.DidNotReceive()
            .DeleteInboundAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private void SetCoreRunning()
    {
        coreStateStore.Set(new RemoteNodeCoreState(
            node.Id,
            true,
            true,
            "1.0.0",
            CoreStatus.Started,
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(1)));
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

    private static InboundEntity CreateInbound(
        long id = 2,
        string tag = "socks-in",
        int port = 10808,
        bool enabled = true,
        bool readOnly = false)
    {
        return new InboundEntity
        {
            Id = id,
            Enabled = enabled,
            ReadOnly = readOnly,
            Config = new SocksInbound
            {
                Tag = tag,
                Port = new Port(port),
                Settings = new Inbound.SocksSettings
                {
                    Auth = SocksAuth.NoAuth,
                    Udp = true
                }
            }
        };
    }
}
