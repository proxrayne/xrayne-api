using System.Text.Json.Serialization;

namespace Contracts.Enums;

/// <summary>
/// Describes the current live remote node connection status.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NodeConnectionStatus
{
    /// <summary>
    /// The panel is connecting or reconnecting to the remote node.
    /// </summary>
    [JsonStringEnumMemberName("connecting")]
    Connecting,

    /// <summary>
    /// The remote node stream is connected.
    /// </summary>
    [JsonStringEnumMemberName("connected")]
    Connected,

    /// <summary>
    /// The remote node stream failed.
    /// </summary>
    [JsonStringEnumMemberName("error")]
    Error,

    /// <summary>
    /// The remote node stream is intentionally disconnected.
    /// </summary>
    [JsonStringEnumMemberName("disconnected")]
    Disconnected
}
