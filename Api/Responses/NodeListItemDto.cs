using Contracts.Enums;

namespace Api.Responses;

/// <summary>
/// Remote node list item response model.
/// </summary>
public sealed record NodeListItemDto(
    long Id,
    string Name,
    string Address,
    int ApiPort,
    NodeConnectionStatus Status,
    string? Message,
    string? NodeVersion,
    NodeListItemXrayDto Xray);
