using Node.Enums;

namespace Node.Models;

/// <summary>
/// Describes the current state of an Xray core installation job.
/// </summary>
public sealed record InstallCoreStatusResponse(
    string JobId,
    InstallCoreStep Step,
    string? Message,
    DateTimeOffset UpdatedAt);
