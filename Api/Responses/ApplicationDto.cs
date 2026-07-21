using Contracts.Enums;

namespace Api.Responses;

/// <summary>
/// Describes a client application profile.
/// </summary>
public sealed record ApplicationDto(
    int Id,
    string Name,
    string? WebsiteUrl,
    string? Protocol,
    string DetectPattern,
    SubscriptionFormat SubscriptionFormat,
    bool Enabled,
    List<string> Assets,
    ImageDto Image,
    List<OperationSystemDto> OperationSystems);
