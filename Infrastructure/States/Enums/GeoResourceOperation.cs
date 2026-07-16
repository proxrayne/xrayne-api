using System.Text.Json.Serialization;

namespace Infrastructure.States;

/// <summary>
/// Defines a queued geo resource background operation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GeoResourceOperation
{
    /// <summary>
    /// Uploads panel-local content to the remote node.
    /// </summary>
    UploadFile,

    /// <summary>
    /// Refreshes the remote file from persisted metadata.
    /// </summary>
    Refresh
}
