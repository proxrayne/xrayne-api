namespace Api.Responses;

/// <summary>
/// Describes a geo resource assigned to a remote node.
/// </summary>
public sealed record NodeGeoResourceDto(
    long Id,
    string FileName,
    long SizeBytes,
    DateTimeOffset LastModifiedAt,
    string SourceType,
    string? Url,
    string? CronTemplate,
    DateTimeOffset? NextRunAt,
    DateTimeOffset? LastErrorAt,
    string? LastError,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

