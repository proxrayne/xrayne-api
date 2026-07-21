using Contracts.Enums;
using OptionalValues;
using OptionalValues.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Defines optional fields used to patch a client application profile.
/// </summary>
public sealed class PatchApplicationRequest
{
    /// <summary>
    /// Gets optional application display name.
    /// </summary>
    [OptionalMaxLength(64)]
    public OptionalValue<string?> Name { get; init; }

    /// <summary>
    /// Gets optional application website URL.
    /// </summary>
    [OptionalMaxLength(64)]
    public OptionalValue<string?> WebsiteUrl { get; init; }

    /// <summary>
    /// Gets optional protocol label used for detection.
    /// </summary>
    [OptionalMaxLength(24)]
    public OptionalValue<string?> Protocol { get; init; }

    /// <summary>
    /// Gets optional pattern used to detect this application.
    /// </summary>
    [OptionalMaxLength(128)]
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
    public OptionalValue<List<string>?> Assets { get; init; }

    /// <summary>
    /// Gets optional linked operating system identifiers.
    /// </summary>
    public OptionalValue<List<string>?> OperationSystemIds { get; init; }
}
