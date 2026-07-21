using OptionalValues;
using OptionalValues.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Defines optional fields used to patch an operating system option.
/// </summary>
public sealed class PatchOperationSystemRequest
{
    /// <summary>
    /// Gets optional operating system display name.
    /// </summary>
    [OptionalMaxLength(64)]
    public OptionalValue<string?> Name { get; init; }

    /// <summary>
    /// Gets optional note.
    /// </summary>
    [OptionalMaxLength(512)]
    public OptionalValue<string?> Note { get; init; }

    /// <summary>
    /// Gets optional enabled state.
    /// </summary>
    public OptionalValue<bool> Enabled { get; init; }
}
