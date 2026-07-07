using AutoMapper;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Api.Controllers;
using Api.Exceptions;
using Api.Mapping;
using Api.Requests;
using Contracts.Configurations;
using Contracts.Enums;
using Contracts.Models;
using Contracts.Utilities;
using Infrastructure.Services;
using Infrastructure.States;
using RemoteNode.Models;
using RemoteNode.Services;
using Data.Entities;
using Data.Implementations;
using Xray.Config.Models;

namespace Test.Controllers;

public sealed class NodesControllerTests
{
    private static readonly Guid TestAdminId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private readonly INodeService _nodes;
    private readonly INodeInboundService _nodeInbounds;
    private readonly INodeSecretService _secrets;
    private readonly IRemoteNodeApiClient _remoteClient;
    private readonly IRemoteNodeApiClientFactory _apiClientFactory;
    private readonly INodeCoreConfigBuilder _coreConfigBuilder;
    private readonly IRemoteNodeConnectionManager _connectionManager;
    private readonly INodeConnectionStateStore _connectionStateStore;
    private readonly IRemoteNodeCoreStateStore _coreStateStore;
    private readonly NodesController _controller;

    public NodesControllerTests()
    {
        _nodes = Substitute.For<INodeService>();
        _nodeInbounds = Substitute.For<INodeInboundService>();
        _secrets = Substitute.For<INodeSecretService>();
        _remoteClient = Substitute.For<IRemoteNodeApiClient>();
        _apiClientFactory = Substitute.For<IRemoteNodeApiClientFactory>();
        _coreConfigBuilder = Substitute.For<INodeCoreConfigBuilder>();
        _connectionManager = Substitute.For<IRemoteNodeConnectionManager>();
        _connectionStateStore = new NodeConnectionStateStore(new MemoryCache(new MemoryCacheOptions()));
        _coreStateStore = Substitute.For<IRemoteNodeCoreStateStore>();
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<NodeMappingProfile>()).CreateMapper();
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Production);
        _secrets.UnprotectApiKey("encrypted").Returns("api-key");
        _apiClientFactory.Create(Arg.Any<RemoteNodeEndpoint>()).Returns(_remoteClient);
        _coreConfigBuilder.Build(Arg.Any<NodeEntity>()).Returns(CreateCoreConfig());

        _controller = new NodesController(
            mapper,
            _nodes,
            _nodeInbounds,
            _secrets,
            Substitute.For<INodeConnectionVerifier>(),
            _connectionManager,
            _apiClientFactory,
            _coreConfigBuilder,
            _connectionStateStore,
            _coreStateStore,
            Substitute.For<IBackgroundTaskScheduler>(),
            Substitute.For<INodeProvisionStateMachine>(),
            Substitute.For<IEventStreamManager>(),
            environment,
            Options.Create(new NodeConnectionOptions()))
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext() },
        };
    }

    [Fact]
    public async Task UpdateConnectionParameters_ThrowsBadRequest_WhenPasswordIsMissing()
    {
        var node = CreateNode();
        node.AuthType = SSHAuthType.PrivateKey;
        node.Password = null;
        node.SSHKey = "saved-key";
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        var request = CreateConnectionParametersRequest(authType: SSHAuthType.Password, password: "");

        var act = () => _controller.UpdateConnectionParameters(1, request, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("SSH password is required for password authentication.");
    }

    [Fact]
    public async Task Update_ThrowsNotFound_WhenNodeIsMissing()
    {
        _nodes.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((NodeEntity?)null);

        var act = () => _controller.Update(1, CreateUpdateRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_ReturnsUpdated_WhenConnectionParametersDoNotChange()
    {
        var node = CreateNode();
        _connectionStateStore.Set(new NodeConnectionState(
            node.Id,
            NodeConnectionStatus.Connected,
            null,
            null));
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        _nodes.UpdateAsync(Arg.Any<NodeEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<NodeEntity>());

        var result = await _controller.Update(
            node.Id,
            CreateUpdateRequest(name: "Updated node", note: "Updated note"),
            CancellationToken.None);

        result.Status.Should().Be("updated");
        result.Node.Name.Should().Be("Updated node");
        result.Node.Note.Should().Be("Updated note");
        result.Node.Port.Should().Be(22);
        result.Node.Status.Should().Be(NodeConnectionStatus.Connected);
        await _connectionManager.DidNotReceive()
            .ReconnectAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateConnectionParameters_QueuesReconnect_WhenConnectionParametersChange()
    {
        var node = CreateNode();
        node.ReconnectAttemptCount = 3;
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        _nodes.UpdateAsync(Arg.Any<NodeEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<NodeEntity>());

        var result = await _controller.UpdateConnectionParameters(
            node.Id,
            CreateConnectionParametersRequest(address: "NODE.EXAMPLE.COM.", apiPort: 9443, password: "new-secret"),
            CancellationToken.None);

        result.Status.Should().Be("reconnect_queued");
        result.Node.Address.Should().Be("node.example.com");
        result.Node.ApiPort.Should().Be(9443);
        result.Node.Status.Should().Be(NodeConnectionStatus.Connecting);
        result.Node.ReconnectAttemptCount.Should().Be(0);
        result.Node.Message.Should().Be("Node connection parameters updated. Manual reconnect requested.");
        await _connectionManager.Received(1).ReconnectAsync(node.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateConnectionParameters_KeepsExistingSecret_WhenSecretIsBlankAndAuthTypeIsUnchanged()
    {
        var node = CreateNode();
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        _nodes.UpdateAsync(Arg.Any<NodeEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<NodeEntity>());

        var result = await _controller.UpdateConnectionParameters(
            node.Id,
            CreateConnectionParametersRequest(apiPort: node.ApiPort, password: ""),
            CancellationToken.None);

        result.Status.Should().Be("updated");
        await _nodes.Received(1).UpdateAsync(
            Arg.Is<NodeEntity>(item => item.Password == "secret"),
            Arg.Any<CancellationToken>());
        await _connectionManager.DidNotReceive()
            .ReconnectAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetConnectionParameters_ReturnsMetadataWithoutSecrets()
    {
        var node = CreateNode();
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);

        var result = await _controller.GetConnectionParameters(node.Id, CancellationToken.None);

        result.Address.Should().Be(node.Address);
        result.ApiPort.Should().Be(node.ApiPort);
        result.HasPassword.Should().BeTrue();
        result.HasSSHKey.Should().BeFalse();
        result.WorkingDirectory.Should().Be(node.WorkingDirectory);
    }

    [Fact]
    public async Task GetConnection_ReturnsCachedState()
    {
        var node = CreateNode();
        var uptime = DateTimeOffset.UtcNow.AddMinutes(-5);
        var snapshot = new NodeConnectionState(
            node.Id,
            NodeConnectionStatus.Connected,
            "1.2.3",
            uptime);
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        _connectionStateStore.Set(snapshot);

        var result = await _controller.GetConnection(node.Id, CancellationToken.None);

        result.State.Should().Be(NodeConnectionStatus.Connected);
        result.ApiVersion.Should().Be("1.2.3");
        result.Uptime.Should().Be(uptime);
    }

    [Fact]
    public async Task GetConnection_ReturnsFallback_WhenSnapshotIsMissing()
    {
        var node = CreateNode();
        node.Enabled = false;
        node.ReconnectAttemptCount = 2;
        node.Message = "Connection failed.";
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);

        var result = await _controller.GetConnection(node.Id, CancellationToken.None);

        result.State.Should().Be(NodeConnectionStatus.Disconnected);
        result.ReconnectAttemptCount.Should().Be(2);
        result.Message.Should().Be("Connection failed.");
        result.ApiVersion.Should().BeNull();
        result.Uptime.Should().BeNull();
    }

    [Fact]
    public async Task GetConnection_ReturnsErrorFallback_WhenReconnectAttemptsAreExhausted()
    {
        var node = CreateNode();
        node.ReconnectAttemptCount = 3;
        node.Message = "Connection failed.";
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);

        var result = await _controller.GetConnection(node.Id, CancellationToken.None);

        result.State.Should().Be(NodeConnectionStatus.Error);
        result.ReconnectAttemptCount.Should().Be(3);
        result.Message.Should().Be("Connection failed.");
    }

    [Fact]
    public async Task GetConnection_ThrowsNotFound_WhenNodeIsMissing()
    {
        _nodes.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((NodeEntity?)null);

        var act = () => _controller.GetConnection(1, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetCoreConfigTemplate_ReturnsTemplate()
    {
        var node = CreateNode();
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);

        var result = await _controller.GetCoreConfigTemplate(node.Id, CancellationToken.None);

        ToJsonObject(result.ConfigTemplate)["log"]!["loglevel"]!.GetValue<string>().Should().Be("warning");
    }

    [Fact]
    public async Task UpdateCoreConfigTemplate_SavesTemplate()
    {
        var node = CreateNode();
        var request = new UpdateNodeConfigTemplateRequest
        {
            ConfigTemplate = """{"log":{"loglevel":"error"},"stats":{}}"""
        };
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        _nodes.UpdateAsync(Arg.Any<NodeEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<NodeEntity>());

        var result = await _controller.UpdateCoreConfigTemplate(node.Id, request, CancellationToken.None);

        ToJsonObject(result.ConfigTemplate)["log"]!["loglevel"]!.GetValue<string>().Should().Be("error");
        await _nodes.Received(1).UpdateAsync(
            Arg.Is<NodeEntity>(item => ToJsonObject(item.ConfigTemplate).ContainsKey("stats")),
            Arg.Any<CancellationToken>());
        await _nodeInbounds.Received(1).SyncReadonlyFromTemplateAsync(
            TestAdminId,
            node,
            Arg.Any<XrayConfig>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartCore_SendsBuiltConfigToRemoteNode()
    {
        var node = CreateNode();
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        _remoteClient.StartCoreAsync(Arg.Any<StartCoreRequest>(), Arg.Any<CancellationToken>())
            .Returns(new OperationAcceptedResponse("start", "queued"));

        var result = await _controller.StartCore(node.Id, CancellationToken.None);

        result.Result.Should().BeOfType<AcceptedResult>();
        await _remoteClient.Received(1).StartCoreAsync(
            Arg.Is<StartCoreRequest>(request => ToJsonObject(request.Config)["log"]!["loglevel"]!.GetValue<string>() == "warning"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RestartCore_SendsBuiltConfigToRemoteNode()
    {
        var node = CreateNode();
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        _remoteClient.RestartCoreAsync(Arg.Any<StartCoreRequest>(), Arg.Any<CancellationToken>())
            .Returns(new OperationAcceptedResponse("restart", "queued"));

        var result = await _controller.RestartCore(node.Id, CancellationToken.None);

        result.Result.Should().BeOfType<AcceptedResult>();
        await _remoteClient.Received(1).RestartCoreAsync(
            Arg.Is<StartCoreRequest>(request => ToJsonObject(request.Config)["log"]!["loglevel"]!.GetValue<string>() == "warning"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Restart_RestartsRemoteNodeAndQueuesReconnect()
    {
        var node = CreateNode();
        node.ReconnectAttemptCount = 2;
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        _nodes.UpdateAsync(Arg.Any<NodeEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<NodeEntity>());
        _remoteClient.RestartRuntimeAsync(Arg.Any<CancellationToken>())
            .Returns(new OperationAcceptedResponse("restart", "queued"));

        var result = await _controller.Restart(node.Id, CancellationToken.None);

        result.Should().BeOfType<AcceptedResult>();
        await _remoteClient.Received(1).RestartRuntimeAsync(Arg.Any<CancellationToken>());
        await _connectionManager.Received(1).ReconnectAsync(node.Id, Arg.Any<CancellationToken>());
        node.ReconnectAttemptCount.Should().Be(0);
        node.Message.Should().Be("Remote node restart requested.");
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
            AuthType = SSHAuthType.Password,
            Password = "secret",
            SSHKey = null,
            WorkingDirectory = "/opt/xrayne",
            Note = "",
            ConfigTemplate = CreateCoreConfig(),
            EncryptedApiKey = "encrypted",
            ApiKeyFingerprint = "fingerprint",
            CertificateMode = CertificateMode.Domain,
            Enabled = true,
            LastStatusChange = DateTime.UtcNow,
            InstallationMessage = "Connected.",
        };
    }

    private static XrayConfig CreateCoreConfig()
    {
        return XrayJsonSerializer.DeserializeRequired<XrayConfig>(
            """{"log":{"loglevel":"warning"}}""",
            "Core config cannot be empty.");
    }

    private static JsonObject ToJsonObject(XrayConfig config)
    {
        var json = XrayJsonSerializer.Serialize(config);
        return JsonNode.Parse(json)!.AsObject();
    }

    private static JsonObject ToJsonObject(string config)
    {
        return JsonNode.Parse(config)!.AsObject();
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, TestAdminId.ToString())],
            "Test"));

        return context;
    }

    private static UpdateNodeRequest CreateUpdateRequest(
        string name = "Node",
        string note = "")
    {
        return new UpdateNodeRequest
        {
            Name = name,
            Note = note,
        };
    }

    private static UpdateNodeConnectionParametersRequest CreateConnectionParametersRequest(
        string address = "node.example.com",
        int port = 22,
        int apiPort = 8443,
        SSHAuthType authType = SSHAuthType.Password,
        string password = "secret",
        string? sshKey = null)
    {
        return new UpdateNodeConnectionParametersRequest
        {
            Address = address,
            Port = port,
            ApiPort = apiPort,
            SSHUsername = "root",
            AuthType = authType,
            Password = password,
            SSHKey = sshKey,
        };
    }
}
