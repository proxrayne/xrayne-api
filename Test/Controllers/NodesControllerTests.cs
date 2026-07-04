using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Api.Controllers;
using Api.Exceptions;
using Api.Mapping;
using Api.Requests;
using Contracts.Configurations;
using Contracts.Enums;
using Infrastructure.Services;
using Infrastructure.States;
using RemoteNode.Models;
using RemoteNode.Services;
using Repositories.Entities;

namespace Test.Controllers;

public sealed class NodesControllerTests
{
    private readonly INodeService _nodes;
    private readonly IRemoteNodeConnectionManager _connectionManager;
    private readonly NodesController _controller;

    public NodesControllerTests()
    {
        _nodes = Substitute.For<INodeService>();
        _connectionManager = Substitute.For<IRemoteNodeConnectionManager>();
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<NodeMappingProfile>()).CreateMapper();
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Production);

        _controller = new NodesController(
            mapper,
            _nodes,
            Substitute.For<INodeSecretService>(),
            Substitute.For<INodeConnectionVerifier>(),
            _connectionManager,
            Substitute.For<IRemoteNodeApiClientFactory>(),
            Substitute.For<IBackgroundTaskScheduler>(),
            Substitute.For<INodeProvisionStateMachine>(),
            Substitute.For<IEventStreamManager>(),
            environment,
            Options.Create(new NodeConnectionOptions()))
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    [Fact]
    public async Task Update_ThrowsBadRequest_WhenPasswordIsMissing()
    {
        var request = CreateUpdateRequest(password: "");

        var act = () => _controller.Update(1, request, CancellationToken.None);

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
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        _nodes.UpdateAsync(Arg.Any<NodeEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<NodeEntity>());

        var result = await _controller.Update(
            node.Id,
            CreateUpdateRequest(name: "Updated node", note: "Updated note", port: 2222),
            CancellationToken.None);

        result.Status.Should().Be("updated");
        result.Node.Name.Should().Be("Updated node");
        result.Node.Note.Should().Be("Updated note");
        result.Node.Port.Should().Be(2222);
        result.Node.Status.Should().Be(NodeStatus.Connected);
        await _connectionManager.DidNotReceive()
            .ReconnectAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_QueuesReconnect_WhenConnectionParametersChange()
    {
        var node = CreateNode();
        node.ReconnectAttemptCount = 3;
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        _nodes.UpdateAsync(Arg.Any<NodeEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<NodeEntity>());

        var result = await _controller.Update(
            node.Id,
            CreateUpdateRequest(address: "NODE.EXAMPLE.COM.", apiPort: 9443, password: "new-secret"),
            CancellationToken.None);

        result.Status.Should().Be("reconnect_queued");
        result.Node.Address.Should().Be("node.example.com");
        result.Node.ApiPort.Should().Be(9443);
        result.Node.Status.Should().Be(NodeStatus.Connecting);
        result.Node.ReconnectAttemptCount.Should().Be(0);
        result.Node.Message.Should().Be("Node connection parameters updated. Manual reconnect requested.");
        await _connectionManager.Received(1).ReconnectAsync(node.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetConnection_ReturnsCachedTelemetry()
    {
        var node = CreateNode();
        var telemetry = new NodePingResponse(
            "1.2.3",
            "Production",
            TimeSpan.FromMinutes(5),
            new NodeCoreStatus(true, true, "25.7.1", "started"));
        var snapshot = new RemoteNodeConnectionSnapshot(
            node.Id,
            RemoteNodeConnectionState.Connected,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            0,
            null,
            telemetry);
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        _connectionManager.GetSnapshot(node.Id).Returns(snapshot);

        var result = await _controller.GetConnection(node.Id, CancellationToken.None);

        result.State.Should().Be(RemoteNodeConnectionState.Connected);
        result.Telemetry.Should().NotBeNull();
        result.Telemetry!.NodeVersion.Should().Be("1.2.3");
        result.Telemetry.Core.IsRunning.Should().BeTrue();
        result.Telemetry.Core.Version.Should().Be("25.7.1");
    }

    [Fact]
    public async Task GetConnection_ReturnsFallback_WhenSnapshotIsMissing()
    {
        var node = CreateNode();
        node.Status = NodeStatus.Error;
        node.ReconnectAttemptCount = 2;
        node.Message = "Connection failed.";
        _nodes.GetByIdAsync(node.Id, Arg.Any<CancellationToken>()).Returns(node);
        _connectionManager.GetSnapshot(node.Id).Returns((RemoteNodeConnectionSnapshot?)null);

        var result = await _controller.GetConnection(node.Id, CancellationToken.None);

        result.State.Should().Be(RemoteNodeConnectionState.Error);
        result.ReconnectAttemptCount.Should().Be(2);
        result.Message.Should().Be("Connection failed.");
        result.Telemetry.Should().BeNull();
    }

    [Fact]
    public async Task GetConnection_ThrowsNotFound_WhenNodeIsMissing()
    {
        _nodes.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((NodeEntity?)null);

        var act = () => _controller.GetConnection(1, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
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
            EncryptedApiKey = "encrypted",
            ApiKeyFingerprint = "fingerprint",
            CertificateMode = CertificateMode.Domain,
            Status = NodeStatus.Connected,
            LastStatusChange = DateTime.UtcNow,
            InstallationMessage = "Connected.",
        };
    }

    private static UpdateNodeRequest CreateUpdateRequest(
        string name = "Node",
        string address = "node.example.com",
        int port = 22,
        int apiPort = 8443,
        string password = "secret",
        string note = "")
    {
        return new UpdateNodeRequest
        {
            Name = name,
            Address = address,
            Port = port,
            ApiPort = apiPort,
            SSHUsername = "root",
            AuthType = SSHAuthType.Password,
            Password = password,
            WorkingDirectory = "/opt/xrayne",
            Note = note,
        };
    }
}
