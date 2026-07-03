namespace Node.Responses;

/// <summary>
/// Describes the current Xray core installation and process state.
/// </summary>
public sealed record CoreStatusResponse(
    bool IsInstalled,
    bool IsRunning,
    string? Version,
    string Status);

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
    string Status,
    string? Detail);

/// <summary>
/// Describes an accepted remote node operation.
/// </summary>
public sealed record OperationAcceptedResponse(
    string Operation,
    string Status);
