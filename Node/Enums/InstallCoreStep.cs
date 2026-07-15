using System.Text.Json.Serialization;

namespace Node.Enums;

/// <summary>
/// Describes a remote xray-core installation step.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InstallCoreStep
{
    /// <summary>
    /// Installation is queued.
    /// </summary>
    [JsonStringEnumMemberName("queued")]
    Queued,

    /// <summary>
    /// Release metadata and assets are being validated.
    /// </summary>
    [JsonStringEnumMemberName("validation")]
    Validation,

    /// <summary>
    /// The release archive is being downloaded.
    /// </summary>
    [JsonStringEnumMemberName("downloading")]
    Downloading,

    /// <summary>
    /// The release archive is being extracted.
    /// </summary>
    [JsonStringEnumMemberName("extracting")]
    Extracting,

    /// <summary>
    /// The extracted core is being activated.
    /// </summary>
    [JsonStringEnumMemberName("installing")]
    Installing,

    /// <summary>
    /// Installation completed successfully.
    /// </summary>
    [JsonStringEnumMemberName("installed")]
    Installed,

    /// <summary>
    /// Installation failed.
    /// </summary>
    [JsonStringEnumMemberName("failure")]
    Failure
}
