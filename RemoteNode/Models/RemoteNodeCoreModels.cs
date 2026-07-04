using System.Text.Json.Serialization;

namespace RemoteNode.Models;

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

/// <summary>
/// Describes the current Xray core installation and process state.
/// </summary>
public sealed record CoreStatusResponse(
    bool IsInstalled,
    CoreStatus? Status,
    bool IsInstalling,
    string? Version);

/// <summary>
/// Requests installation of a specific Xray core version.
/// </summary>
public sealed record InstallCoreRequest(string? Version);

/// <summary>
/// Describes an accepted Xray core installation job.
/// </summary>
public sealed record InstallCoreResponse(
    string JobId,
    string Version,
    string Status);

/// <summary>
/// Describes the current state of an Xray core installation job.
/// </summary>
public sealed record InstallCoreStatusResponse(
    string JobId,
    InstallCoreStep Step,
    string? Message,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Describes an accepted remote node operation.
/// </summary>
public sealed record OperationAcceptedResponse(
    string Operation,
    string Status);

/// <summary>
/// Describes remote node system status.
/// </summary>
public sealed record SystemStatusResponse(
    DateTimeOffset Timestamp,
    NodeSystemStats System);
