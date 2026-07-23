using Contracts.Enums;

namespace Api.Responses;

/// <summary>
/// Describes cached xray-core telemetry for a remote node list item.
/// </summary>
public sealed record NodeListItemXrayDto(
    bool IsInstalled,
    bool IsRunning,
    string? Version,
    CoreStatus? Status);
