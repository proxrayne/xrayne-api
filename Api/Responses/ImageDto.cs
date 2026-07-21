namespace Api.Responses;

/// <summary>
/// Describes image metadata returned to API clients.
/// </summary>
public sealed record ImageDto(
    string? Alt,
    string Url);
