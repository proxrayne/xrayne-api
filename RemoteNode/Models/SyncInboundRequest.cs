using System.Text.Json.Serialization;
using RemoteNode.Models.Json;

namespace RemoteNode.Models;

/// <summary>
/// Requests synchronization of a single inbound configuration with the running core.
/// </summary>
[JsonConverter(typeof(SyncInboundRequestJsonConverter))]
public sealed class SyncInboundRequest : InboundSyncItem;
