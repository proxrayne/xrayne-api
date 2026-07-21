using OptionalValues;
using OptionalValues.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Defines optional fields used to patch a subscription host.
/// </summary>
public sealed record PatchHostRequest
{
    /// <summary>
    /// Gets optional host display name.
    /// </summary>
    [OptionalMaxLength(128)]
    public OptionalValue<string?> Name { get; init; }

    /// <summary>
    /// Gets optional host address.
    /// </summary>
    [OptionalMaxLength(128)]
    public OptionalValue<string?> Address { get; init; }

    /// <summary>
    /// Gets optional ISO 3166-1 alpha-2 country code.
    /// </summary>
    [OptionalMinLength(2)]
    [OptionalMaxLength(2)]
    public OptionalValue<string?> CountryAlpha2Code { get; init; }

    /// <summary>
    /// Gets optional inbound identifier.
    /// </summary>
    public OptionalValue<long> InboundId { get; init; }

    /// <summary>
    /// Gets optional port override.
    /// </summary>
    public OptionalValue<int?> Port { get; init; }

    /// <summary>
    /// Gets optional TLS server name override.
    /// </summary>
    [OptionalMaxLength(128)]
    public OptionalValue<string?> ServerName { get; init; }

    /// <summary>
    /// Gets optional HTTP host override.
    /// </summary>
    [OptionalMaxLength(128)]
    public OptionalValue<string?> Host { get; init; }

    /// <summary>
    /// Gets optional request path override.
    /// </summary>
    [OptionalMaxLength(256)]
    public OptionalValue<string?> Path { get; init; }

    /// <summary>
    /// Gets optional security mode.
    /// </summary>
    public OptionalValue<string?> Security { get; init; }

    /// <summary>
    /// Gets optional ALPN values.
    /// </summary>
    public OptionalValue<List<string>?> Alpn { get; init; }

    /// <summary>
    /// Gets optional TLS fingerprint.
    /// </summary>
    public OptionalValue<string?> Fingerprint { get; init; }

    /// <summary>
    /// Gets optional fragment template.
    /// </summary>
    [OptionalMaxLength(100)]
    public OptionalValue<string?> FragmentTemplate { get; init; }

    /// <summary>
    /// Gets optional noise template.
    /// </summary>
    [OptionalMaxLength(2000)]
    public OptionalValue<string?> NoiseTemplate { get; init; }

    /// <summary>
    /// Gets optional enabled state.
    /// </summary>
    public OptionalValue<bool> Enabled { get; init; }

    /// <summary>
    /// Gets optional mux state.
    /// </summary>
    public OptionalValue<bool> IsMuxEnabled { get; init; }

    /// <summary>
    /// Gets optional SNI-as-host state.
    /// </summary>
    public OptionalValue<bool> IsUseServerNameAsHost { get; init; }

    /// <summary>
    /// Gets optional random user agent state.
    /// </summary>
    public OptionalValue<bool> IsRandomUseragent { get; init; }

    /// <summary>
    /// Gets optional allow-increase state.
    /// </summary>
    public OptionalValue<bool> AllowIncrease { get; init; }
}
