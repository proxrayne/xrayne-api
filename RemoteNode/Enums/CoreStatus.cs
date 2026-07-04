using System.Text.Json.Serialization;

namespace RemoteNode.Enums;

/// <summary>
/// Describes a high-level remote xray-core runtime state.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CoreStatus
{
    /// <summary>
    /// xray-core is starting.
    /// </summary>
    [JsonStringEnumMemberName("starting")]
    Starting,

    /// <summary>
    /// xray-core is running.
    /// </summary>
    [JsonStringEnumMemberName("started")]
    Started,

    /// <summary>
    /// xray-core is stopping.
    /// </summary>
    [JsonStringEnumMemberName("stopping")]
    Stopping,

    /// <summary>
    /// xray-core is installed but stopped.
    /// </summary>
    [JsonStringEnumMemberName("stopped")]
    Stopped,

    /// <summary>
    /// xray-core is restarting.
    /// </summary>
    [JsonStringEnumMemberName("restarting")]
    Restarting
}
