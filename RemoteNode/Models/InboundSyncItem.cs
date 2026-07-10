using Xray.Config.Models;
using System.Text.Json.Serialization;
using RemoteNode.Models.Json;

namespace RemoteNode.Models;

/// <summary>
/// Carries a managed inbound configuration slice.
/// </summary>
[JsonConverter(typeof(InboundSyncItemJsonConverter))]
public class InboundSyncItem
{
    /// <summary>
    /// Gets the panel-owned inbound identifier.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Gets the inbound position in the effective xray-core configuration.
    /// </summary>
    public int Position { get; init; }

    /// <summary>
    /// Gets the inbound configuration.
    /// </summary>
    public required Inbound Inbound { get; init; }
}
