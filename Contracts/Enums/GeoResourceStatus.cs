using System.Text.Json.Serialization;

namespace Contracts.Enums;

/// <summary>
/// Defines the persisted processing state of a node geo resource file.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GeoResourceStatus
{
    /// <summary>
    /// Geo resource processing is queued.
    /// </summary>
    [JsonStringEnumMemberName("queued")]
    Queued,

    /// <summary>
    /// Geo resource metadata or remote file state is being updated.
    /// </summary>
    [JsonStringEnumMemberName("updating")]
    Updating,

    /// <summary>
    /// Geo resource content is being loaded by the panel.
    /// </summary>
    [JsonStringEnumMemberName("loading")]
    Loading,

    /// <summary>
    /// Geo resource content is being transferred to the remote node.
    /// </summary>
    [JsonStringEnumMemberName("transferring")]
    Transferring,

    /// <summary>
    /// Geo resource processing failed.
    /// </summary>
    [JsonStringEnumMemberName("error")]
    Error,

    /// <summary>
    /// Geo resource is available on the remote node.
    /// </summary>
    [JsonStringEnumMemberName("success")]
    Success
}
