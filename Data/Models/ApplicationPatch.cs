using Contracts.Enums;
using OptionalValues;

namespace Data.Models;

/// <summary>
/// Describes optional client application fields to update.
/// </summary>
public sealed class ApplicationPatch
{
    /// <summary>
    /// Gets optional application display name.
    /// </summary>
    public OptionalValue<string?> Name { get; init; }

    /// <summary>
    /// Gets optional application website URL.
    /// </summary>
    public OptionalValue<string?> WebsiteUrl { get; init; }

    /// <summary>
    /// Gets optional protocol label used for detection.
    /// </summary>
    public OptionalValue<string?> Protocol { get; init; }

    /// <summary>
    /// Gets optional pattern used to detect this application.
    /// </summary>
    public OptionalValue<string?> DetectPattern { get; init; }

    /// <summary>
    /// Gets optional subscription format produced for this application.
    /// </summary>
    public OptionalValue<SubscriptionFormat> SubscriptionFormat { get; init; }

    /// <summary>
    /// Gets optional enabled state.
    /// </summary>
    public OptionalValue<bool> Enabled { get; init; }

    /// <summary>
    /// Gets optional application asset references.
    /// </summary>
    public OptionalValue<IReadOnlyCollection<string>?> Assets { get; init; }

    /// <summary>
    /// Gets optional linked operating system identifiers.
    /// </summary>
    public OptionalValue<IReadOnlyCollection<string>?> OperationSystemIds { get; init; }
}
