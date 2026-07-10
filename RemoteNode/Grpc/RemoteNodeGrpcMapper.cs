using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using RemoteNode.Enums;
using RemoteNode.Exceptions;
using RemoteNode.Models;
using Xray.Config.Models;
using Proto = XRayne.ProtoTypes.RemoteNode.V1;

namespace RemoteNode.Grpc;

/// <summary>
/// Maps panel remote-node contracts to the gRPC transport contract.
/// </summary>
public static class RemoteNodeGrpcMapper
{
    /// <summary>
    /// Converts node telemetry from the gRPC contract.
    /// </summary>
    public static NodePingResponse ToDomain(Proto.NodePingResponse value)
    {
        return new NodePingResponse(
            value.NodeVersion,
            value.Environment,
            ToTimeSpan(value.UptimeSeconds),
            ToDomain(value.Core));
    }

    /// <summary>
    /// Converts system status from the gRPC contract.
    /// </summary>
    public static SystemStatusResponse ToDomain(Proto.SystemStatusResponse value)
    {
        return new SystemStatusResponse(ToDateTimeOffset(value.Timestamp), ToDomain(value.System));
    }

    /// <summary>
    /// Converts xray-core status from the gRPC contract.
    /// </summary>
    public static CoreStatusResponse ToDomain(Proto.CoreStatusResponse value)
    {
        return new CoreStatusResponse(
            value.IsInstalled,
            value.HasStatus ? ToDomain(value.Status) : null,
            value.IsInstalling,
            value.HasVersion ? value.Version : null,
            value.StartedAt is null ? null : ToDateTimeOffset(value.StartedAt),
            value.HasUptimeSeconds ? ToTimeSpan(value.UptimeSeconds) : null);
    }

    /// <summary>
    /// Converts an install response from the gRPC contract.
    /// </summary>
    public static InstallCoreResponse ToDomain(Proto.InstallCoreResponse value)
    {
        return new InstallCoreResponse(value.JobId, value.Version, value.Status);
    }

    /// <summary>
    /// Converts install status from the gRPC contract.
    /// </summary>
    public static InstallCoreStatusResponse ToDomain(Proto.InstallCoreStatusResponse value)
    {
        return new InstallCoreStatusResponse(
            value.JobId,
            ToDomain(value.Step),
            value.HasMessage ? value.Message : null,
            ToDateTimeOffset(value.UpdatedAt));
    }

    /// <summary>
    /// Converts an accepted operation from the gRPC contract.
    /// </summary>
    public static OperationAcceptedResponse ToDomain(Proto.OperationAcceptedResponse value)
    {
        return new OperationAcceptedResponse(value.Operation, value.Status);
    }

    /// <summary>
    /// Converts a remote log stream event from the gRPC contract.
    /// </summary>
    public static RemoteLogStreamEvent ToDomain(Proto.RemoteLogStreamEvent value)
    {
        return new RemoteLogStreamEvent(
            value.Type,
            value.Entries.Count == 0 ? null : [.. value.Entries.Select(ToDomain)],
            value.Entry is null ? null : ToDomain(value.Entry));
    }

    /// <summary>
    /// Converts a node connection event from the gRPC contract.
    /// </summary>
    public static NodeConnectionEvent ToDomain(Proto.NodeConnectionEvent value)
    {
        return new NodeConnectionEvent(
            value.Type,
            ToDateTimeOffset(value.Timestamp),
            value.Ping is null ? null : ToDomain(value.Ping),
            value.Core is null ? null : ToDomain(value.Core),
            value.Install is null ? null : ToDomain(value.Install),
            value.Log is null ? null : ToDomain(value.Log));
    }

    /// <summary>
    /// Converts geo resource metadata from the gRPC contract.
    /// </summary>
    public static GeoResourceDto ToDomain(Proto.GeoResourceDto value)
    {
        return new GeoResourceDto(value.FileName, value.SizeBytes, ToDateTimeOffset(value.LastModifiedAt));
    }

    /// <summary>
    /// Converts certificate metadata from the gRPC contract.
    /// </summary>
    public static CertificateDto ToDomain(Proto.CertificateDto value)
    {
        return new CertificateDto(
            value.Domain,
            value.Active,
            ToDateTimeOffset(value.ExpireAt),
            value.CertificateFile,
            value.PrivateKeyFile);
    }

    /// <summary>
    /// Converts a start request to the gRPC contract.
    /// </summary>
    public static Proto.StartCoreRequest ToProto(StartCoreRequest value)
    {
        var request = new Proto.StartCoreRequest
        {
            ConfigTemplateJson = SerializeXray(value.ConfigTemplate)
        };

        request.Inbounds.AddRange(value.Inbounds.Select(ToProto));
        request.Outbounds.AddRange(value.Outbounds.Select(ToProto));
        request.RoutingRules.AddRange(value.RoutingRules.Select(ToProto));

        return request;
    }

    /// <summary>
    /// Converts a config template request to the gRPC contract.
    /// </summary>
    public static Proto.UpdateCoreConfigTemplateRequest ToProto(UpdateCoreConfigTemplateRequest value)
    {
        return new Proto.UpdateCoreConfigTemplateRequest
        {
            ConfigTemplateJson = SerializeXray(value.ConfigTemplate)
        };
    }

    /// <summary>
    /// Converts an inbound sync request to the gRPC contract.
    /// </summary>
    public static Proto.SyncInboundRequest ToProto(SyncInboundRequest value)
    {
        return new Proto.SyncInboundRequest
        {
            Inbound = ToProto((InboundSyncItem)value)
        };
    }

    /// <summary>
    /// Converts an outbound sync request to the gRPC contract.
    /// </summary>
    public static Proto.SyncOutboundRequest ToProto(SyncOutboundRequest value)
    {
        return new Proto.SyncOutboundRequest
        {
            Outbound = ToProto((OutboundSyncItem)value)
        };
    }

    /// <summary>
    /// Converts routing rule sync request to the gRPC contract.
    /// </summary>
    public static Proto.SyncRoutingRulesRequest ToProto(SyncRoutingRulesRequest value)
    {
        var request = new Proto.SyncRoutingRulesRequest();
        request.RoutingRules.AddRange(value.RoutingRules.Select(ToProto));

        return request;
    }

    private static NodeCoreStatus ToDomain(Proto.NodeCoreStatus value)
    {
        return new NodeCoreStatus(
            value.IsInstalled,
            value.IsRunning,
            value.HasVersion ? value.Version : null,
            value.Status);
    }

    private static NodeSystemStats ToDomain(Proto.NodeSystemStats value)
    {
        return new NodeSystemStats(
            value.MachineName,
            value.OsDescription,
            value.ProcessorCount,
            value.WorkingSetBytes,
            value.GcTotalMemoryBytes,
            value.CurrentProcessThreadCount,
            value.HasSystemThreadCount ? value.SystemThreadCount : null,
            ToDateTimeOffset(value.StartedAt),
            ToDateTimeOffset(value.Timestamp),
            ToTimeSpan(value.UptimeSeconds),
            ToDomain(value.Cpu),
            ToDomain(value.Memory),
            ToDomain(value.Swap),
            [.. value.Volumes.Select(ToDomain)],
            ToDomain(value.Network));
    }

    private static NodeCpuStats ToDomain(Proto.NodeCpuStats value)
    {
        return new NodeCpuStats(
            value.LogicalCoreCount,
            value.HasAverageUsagePercent ? value.AverageUsagePercent : null,
            [.. value.Cores.Select(ToDomain)]);
    }

    private static NodeCpuCoreUsage ToDomain(Proto.NodeCpuCoreUsage value)
    {
        return new NodeCpuCoreUsage(value.Index, value.HasUsagePercent ? value.UsagePercent : null);
    }

    private static NodeMemoryStats ToDomain(Proto.NodeMemoryStats value)
    {
        return new NodeMemoryStats(value.TotalBytes, value.UsedBytes, value.AvailableBytes);
    }

    private static NodeVolumeStats ToDomain(Proto.NodeVolumeStats value)
    {
        return new NodeVolumeStats(
            value.Name,
            value.FileSystem,
            value.TotalBytes,
            value.FreeBytes,
            value.UsedBytes,
            value.UsedPercent);
    }

    private static NodeNetworkStats ToDomain(Proto.NodeNetworkStats value)
    {
        return new NodeNetworkStats(value.Ipv4Addresses, value.Ipv6Addresses);
    }

    /// <summary>
    /// Converts a remote log entry from the gRPC contract.
    /// </summary>
    public static RemoteLogEntry ToDomain(Proto.RemoteLogEntry value)
    {
        return new RemoteLogEntry(
            value.Id,
            ToDateTimeOffset(value.Timestamp),
            value.Level,
            value.Message);
    }

    private static Proto.ManagedInbound ToProto(InboundSyncItem value)
    {
        return new Proto.ManagedInbound
        {
            Id = value.Id,
            Position = value.Position,
            InboundJson = SerializeXray(value.Inbound)
        };
    }

    private static Proto.ManagedOutbound ToProto(OutboundSyncItem value)
    {
        return new Proto.ManagedOutbound
        {
            Id = value.Id,
            Position = value.Position,
            OutboundJson = SerializeXray(value.Outbound)
        };
    }

    private static Proto.ManagedRoutingRule ToProto(RoutingRuleSyncItem value)
    {
        return new Proto.ManagedRoutingRule
        {
            Id = value.Id,
            Position = value.Position,
            RoutingRuleJson = SerializeXray(value.RoutingRule)
        };
    }

    private static RemoteCoreStatus ToDomain(Proto.RemoteCoreStatus value)
    {
        return value switch
        {
            Proto.RemoteCoreStatus.Starting => RemoteCoreStatus.Starting,
            Proto.RemoteCoreStatus.Started => RemoteCoreStatus.Started,
            Proto.RemoteCoreStatus.Stopping => RemoteCoreStatus.Stopping,
            Proto.RemoteCoreStatus.Stopped => RemoteCoreStatus.Stopped,
            Proto.RemoteCoreStatus.Restarting => RemoteCoreStatus.Restarting,
            _ => throw new RemoteNodeProtocolException(0, "grpc", $"Unknown core status '{value}'.")
        };
    }

    private static InstallCoreStep ToDomain(Proto.InstallCoreStep value)
    {
        return value switch
        {
            Proto.InstallCoreStep.Queued => InstallCoreStep.Queued,
            Proto.InstallCoreStep.Validation => InstallCoreStep.Validation,
            Proto.InstallCoreStep.Downloading => InstallCoreStep.Downloading,
            Proto.InstallCoreStep.Extracting => InstallCoreStep.Extracting,
            Proto.InstallCoreStep.Installing => InstallCoreStep.Installing,
            Proto.InstallCoreStep.Installed => InstallCoreStep.Installed,
            Proto.InstallCoreStep.Failure => InstallCoreStep.Failure,
            _ => throw new RemoteNodeProtocolException(0, "grpc", $"Unknown install step '{value}'.")
        };
    }

    private static DateTimeOffset ToDateTimeOffset(Timestamp value)
    {
        return new DateTimeOffset(value.ToDateTime());
    }

    private static TimeSpan ToTimeSpan(long seconds)
    {
        return TimeSpan.FromSeconds(Math.Max(0, seconds));
    }

    private static string SerializeXray<T>(T value)
    {
        return JsonSerializer.Serialize(value, XrayConfig.JsonSerializationOptions);
    }
}
