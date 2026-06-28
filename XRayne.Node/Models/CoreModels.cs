namespace XRayne.Node.Models;

public sealed record CoreStatusResponse(
    bool IsInstalled,
    bool IsRunning,
    string? Version,
    string Status);

public sealed record InstallCoreRequest(string? Version);

public sealed record InstallCoreResponse(
    string JobId,
    string Version,
    string Status);

public sealed record InstallCoreStatusResponse(
    string JobId,
    string Status,
    string? Detail);

public sealed record OperationAcceptedResponse(
    string Operation,
    string Status);
