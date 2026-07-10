using System.Text.Json.Serialization;
using RemoteNode.Models.Json;

namespace RemoteNode.Models;

/// <summary>
/// Requests synchronization of a single outbound configuration with the running core.
/// </summary>
[JsonConverter(typeof(SyncOutboundRequestJsonConverter))]
public sealed class SyncOutboundRequest : OutboundSyncItem;
