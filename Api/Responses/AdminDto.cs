namespace Api.Responses;

/// <summary>
/// Represents an administrator account.
/// </summary>
public sealed record AdminDto(
    long Id,
    string Username,
    string? Email,
    IReadOnlyCollection<string> Permissions,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastLoginAt);
