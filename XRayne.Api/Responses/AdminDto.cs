namespace XRayne.Api.Responses;

public sealed record AdminDto(
    Guid Id,
    string Username,
    string? Email,
    IReadOnlyCollection<string> Permissions,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastLoginAt);
