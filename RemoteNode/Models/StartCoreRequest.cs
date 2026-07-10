using Xray.Config.Models;
using System.Text.Json.Serialization;
using RemoteNode.Models.Json;

namespace RemoteNode.Models;

/// <summary>
/// Requests xray-core start with a structured runtime configuration snapshot.
/// </summary>
[JsonConverter(typeof(StartCoreRequestJsonConverter))]
public sealed class StartCoreRequest
{
    /// <summary>
    /// Gets the base xray-core configuration template without managed slices.
    /// </summary>
    public required XrayConfig ConfigTemplate { get; init; }

    /// <summary>
    /// Gets managed inbound configurations ordered by position.
    /// </summary>
    public List<InboundSyncItem> Inbounds { get; init; } = [];

    /// <summary>
    /// Gets managed outbound configurations ordered by position.
    /// </summary>
    public List<OutboundSyncItem> Outbounds { get; init; } = [];

    /// <summary>
    /// Gets managed routing rules ordered by position.
    /// </summary>
    public List<RoutingRuleSyncItem> RoutingRules { get; init; } = [];
}
