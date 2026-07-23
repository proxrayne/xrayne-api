using Api.Mapping;
using Api.Responses;
using AutoMapper;
using Contracts.Enums;
using Contracts.Models;
using Data.Entities;

namespace Test.Mapping;

/// <summary>
/// Tests remote node API mapping rules.
/// </summary>
public sealed class NodeMappingProfileTests
{
    private readonly IMapper _mapper;

    public NodeMappingProfileTests()
    {
        var config = MapperTestFactory.CreateConfiguration(cfg => cfg.AddProfile<NodeMappingProfile>());
        config.AssertConfigurationIsValid();
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void NodeEntity_MapsToNodeListItemDto_WithCachedTelemetry()
    {
        var node = new NodeEntity
        {
            Id = 42,
            Name = "alpha",
            Address = "node.example.com",
            ApiPort = 62050,
            EncryptedApiKey = "protected-key",
            ApiKeyFingerprint = "fingerprint",
            Message = "Connection failed"
        };
        var connection = new NodeConnectionState(
            node.Id,
            NodeConnectionStatus.Error,
            "0.2.0+abc123",
            DateTimeOffset.UtcNow);
        var core = new NodeCoreState(
            node.Id,
            true,
            true,
            "25.7.1+def456",
            CoreStatus.Started,
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(5));

        var dto = _mapper.Map<NodeListItemDto>(
            node,
            options =>
            {
                options.Items[NodeMappingProfile.ConnectionStateItemKey] = connection;
                options.Items[NodeMappingProfile.CoreStateItemKey] = core;
            });

        dto.Id.Should().Be(node.Id);
        dto.Name.Should().Be(node.Name);
        dto.Address.Should().Be(node.Address);
        dto.ApiPort.Should().Be(node.ApiPort);
        dto.Status.Should().Be(NodeConnectionStatus.Error);
        dto.Message.Should().Be(node.Message);
        dto.NodeVersion.Should().Be("0.2.0+abc123");
        dto.Xray.IsInstalled.Should().BeTrue();
        dto.Xray.IsRunning.Should().BeTrue();
        dto.Xray.Version.Should().Be("25.7.1+def456");
        dto.Xray.Status.Should().Be(CoreStatus.Started);
    }

    [Fact]
    public void NodeEntity_MapsToNodeListItemDto_WithoutCachedCoreState()
    {
        var node = new NodeEntity
        {
            Id = 42,
            Name = "alpha",
            Address = "node.example.com",
            ApiPort = 62050,
            EncryptedApiKey = "protected-key",
            ApiKeyFingerprint = "fingerprint"
        };
        var connection = new NodeConnectionState(
            node.Id,
            NodeConnectionStatus.Disconnected,
            null,
            null);

        var dto = _mapper.Map<NodeListItemDto>(
            node,
            options =>
            {
                options.Items[NodeMappingProfile.ConnectionStateItemKey] = connection;
                options.Items[NodeMappingProfile.CoreStateItemKey] = null;
            });

        dto.Status.Should().Be(NodeConnectionStatus.Disconnected);
        dto.NodeVersion.Should().BeNull();
        dto.Xray.IsInstalled.Should().BeFalse();
        dto.Xray.IsRunning.Should().BeFalse();
        dto.Xray.Version.Should().BeNull();
        dto.Xray.Status.Should().BeNull();
    }
}
