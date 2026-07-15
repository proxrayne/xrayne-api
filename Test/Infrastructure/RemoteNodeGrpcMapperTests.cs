using Google.Protobuf.WellKnownTypes;
using RemoteNode.Enums;
using RemoteNode.Grpc;
using RemoteNode.Models;
using Xray.Config.Models;
using Proto = XRayne.ProtoTypes.RemoteNode.V1;

namespace Test.Infrastructure;

public sealed class RemoteNodeGrpcMapperTests
{
    [Fact]
    public void ToDomain_CoreStatus_MapsOptionalFields()
    {
        var startedAt = DateTime.SpecifyKind(new DateTime(2026, 7, 10, 12, 0, 0), DateTimeKind.Utc);
        var response = new Proto.CoreStatusResponse
        {
            IsInstalled = true,
            Status = Proto.RemoteCoreStatus.Started,
            IsInstalling = false,
            Version = "25.6.8",
            StartedAt = Timestamp.FromDateTime(startedAt),
            UptimeSeconds = 42
        };

        var result = RemoteNodeGrpcMapper.ToDomain(response);

        result.IsInstalled.Should().BeTrue();
        result.Status.Should().Be(RemoteCoreStatus.Started);
        result.Version.Should().Be("25.6.8");
        result.StartedAt.Should().Be(new DateTimeOffset(startedAt));
        result.Uptime.Should().Be(TimeSpan.FromSeconds(42));
    }

    [Fact]
    public void ToProto_UpdateCoreConfigTemplate_SerializesConfigAsJsonString()
    {
        var request = new UpdateCoreConfigTemplateRequest
        {
            ConfigTemplate = new XrayConfig()
        };

        var result = RemoteNodeGrpcMapper.ToProto(request);

        result.ConfigTemplateJson.Should().NotBeNullOrWhiteSpace();
        result.ConfigTemplateJson.Should().StartWith("{");
    }

    [Fact]
    public void ToDomain_LogStreamEvent_MapsSnapshotEntries()
    {
        var timestamp = DateTime.SpecifyKind(new DateTime(2026, 7, 10, 13, 30, 0), DateTimeKind.Utc);
        var response = new Proto.RemoteLogStreamEvent
        {
            Type = "snapshot"
        };
        response.Entries.Add(new Proto.RemoteLogEntry
        {
            Id = "log-1",
            Timestamp = Timestamp.FromDateTime(timestamp),
            Level = "information",
            Message = "started"
        });

        var result = RemoteNodeGrpcMapper.ToDomain(response);

        result.Type.Should().Be("snapshot");
        result.Entries.Should().ContainSingle();
        result.Entries![0].Timestamp.Should().Be(new DateTimeOffset(timestamp));
        result.Entries[0].Message.Should().Be("started");
    }

    [Fact]
    public void ToDomain_ConnectionEvent_MapsStreamMetadata()
    {
        var timestamp = DateTime.SpecifyKind(new DateTime(2026, 7, 10, 14, 0, 0), DateTimeKind.Utc);
        var response = new Proto.ConnectionEvent
        {
            EventType = Proto.StreamEventType.CoreStatus,
            Timestamp = Timestamp.FromDateTime(timestamp),
            Sequence = 12,
            DroppedCount = 3,
            Source = "node",
            Core = new Proto.CoreStatusResponse
            {
                IsInstalled = true,
                Status = Proto.RemoteCoreStatus.Started
            }
        };

        var result = RemoteNodeGrpcMapper.ToDomain(response);

        result.Type.Should().Be("core_status");
        result.Sequence.Should().Be(12);
        result.DroppedCount.Should().Be(3);
        result.Source.Should().Be("node");
        result.Core.Should().NotBeNull();
    }
}
