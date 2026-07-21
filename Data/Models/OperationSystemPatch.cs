using OptionalValues;

namespace Data.Models;

/// <summary>
/// Describes optional operating system fields to update.
/// </summary>
public sealed class OperationSystemPatch
{
    /// <summary>
    /// Gets optional operating system display name.
    /// </summary>
    public OptionalValue<string?> Name { get; init; }

    /// <summary>
    /// Gets optional note.
    /// </summary>
    public OptionalValue<string?> Note { get; init; }

    /// <summary>
    /// Gets optional enabled state.
    /// </summary>
    public OptionalValue<bool> Enabled { get; init; }
}
