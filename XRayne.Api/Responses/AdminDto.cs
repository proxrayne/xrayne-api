namespace XRayne.Api.Responses;

public sealed record AdminDto(
    Guid Id,
    string Username,
    IReadOnlyCollection<string> Permissions,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastLoginAt);
