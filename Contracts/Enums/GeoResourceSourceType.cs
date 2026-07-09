using System.Text.Json.Serialization;

namespace Contracts.Enums;

/// <summary>
/// Defines how a node geo resource is managed.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GeoResourceSourceType
{
    /// <summary>
    /// Geo resource is managed manually as an uploaded file.
    /// </summary>
    [JsonStringEnumMemberName("static")]
    Static,

    /// <summary>
    /// Geo resource is refreshed from a URL by a scheduled job.
    /// </summary>
    [JsonStringEnumMemberName("autoUpdate")]
    AutoUpdate
}
