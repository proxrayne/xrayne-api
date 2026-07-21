using OptionalValues;
using OptionalValues.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Defines optional fields used to patch an administrator account.
/// </summary>
public sealed class PatchAdminRequest
{
    /// <summary>
    /// Gets optional administrator username.
    /// </summary>
    [OptionalMaxLength(128)]
    public OptionalValue<string?> Username { get; init; }

    /// <summary>
    /// Gets optional administrator email address.
    /// </summary>
    [OptionalMaxLength(320)]
    public OptionalValue<string?> Email { get; init; }

    /// <summary>
    /// Gets optional comma-separated administrator permissions.
    /// </summary>
    public OptionalValue<string?> Permissions { get; init; }
}
