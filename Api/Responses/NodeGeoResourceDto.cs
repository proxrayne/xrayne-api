using Contracts.Enums;

namespace Api.Responses;

/// <summary>
/// Describes a geo resource assigned to a remote node.
/// </summary>
public sealed record NodeGeoResourceDto(
    long Id,
    string FileName,
    long SizeBytes,
    DateTimeOffset LastModifiedAt,
    bool IsAutoUpdate,
    GeoResourceStatus Status,
    string? StatusMessage,
    string? Url,
    int? UpdateInterval,
    DateTimeOffset? NextRunAt,
    DateTimeOffset? LastErrorAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
