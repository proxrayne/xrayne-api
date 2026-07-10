using System.Text.Json.Serialization;

namespace Contracts.Enums;

/// <summary>
/// Defines sortable user list columns.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserSortBy
{
    /// <summary>
    /// Sort users by creation date.
    /// </summary>
    [JsonStringEnumMemberName("created_at")]
    CreatedAt,

    /// <summary>
    /// Sort users by username.
    /// </summary>
    [JsonStringEnumMemberName("username")]
    Username,

    /// <summary>
    /// Sort users by status.
    /// </summary>
    [JsonStringEnumMemberName("status")]
    Status,

    /// <summary>
    /// Sort users by configured traffic limit.
    /// </summary>
    [JsonStringEnumMemberName("traffic")]
    Traffic,

    /// <summary>
    /// Sort users by connection count.
    /// </summary>
    [JsonStringEnumMemberName("connections")]
    Connections
}
