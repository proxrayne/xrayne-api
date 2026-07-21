using Contracts.Enums;
using OptionalValues;

namespace Data.Models;

/// <summary>
/// Describes optional administrator account fields to update.
/// </summary>
public sealed class AdminAccountPatch
{
    /// <summary>
    /// Gets optional administrator username.
    /// </summary>
    public OptionalValue<string?> Username { get; init; }

    /// <summary>
    /// Gets optional administrator email address.
    /// </summary>
    public OptionalValue<string?> Email { get; init; }

    /// <summary>
    /// Gets optional administrator permissions.
    /// </summary>
    public OptionalValue<AdminPermission> Permissions { get; init; }
}
