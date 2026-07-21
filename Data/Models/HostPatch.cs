using Contracts.Enums;
using OptionalValues;
using Xray.Config.Enums;

namespace Data.Models;

/// <summary>
/// Describes optional host fields to update.
/// </summary>
public sealed class HostPatch
{
    /// <summary>
    /// Gets optional host display name.
    /// </summary>
    public OptionalValue<string?> Name { get; init; }

    /// <summary>
    /// Gets optional host address.
    /// </summary>
    public OptionalValue<string?> Address { get; init; }

    /// <summary>
    /// Gets optional ISO 3166-1 alpha-2 country code.
    /// </summary>
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
    public OptionalValue<string?> ServerName { get; init; }

    /// <summary>
    /// Gets optional HTTP host override.
    /// </summary>
    public OptionalValue<string?> Host { get; init; }

    /// <summary>
    /// Gets optional request path override.
    /// </summary>
    public OptionalValue<string?> Path { get; init; }

    /// <summary>
    /// Gets optional security mode.
    /// </summary>
    public OptionalValue<HostSecurity> Security { get; init; }

    /// <summary>
    /// Gets optional ALPN flag bitmask.
    /// </summary>
    public OptionalValue<ALPN?> ALPN { get; init; }

    /// <summary>
    /// Gets optional TLS fingerprint.
    /// </summary>
    public OptionalValue<Fingerprint> Fingerprint { get; init; }

    /// <summary>
    /// Gets optional fragment template.
    /// </summary>
    public OptionalValue<string?> FragmentTemplate { get; init; }

    /// <summary>
    /// Gets optional noise template.
    /// </summary>
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
