using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Defines values used to create a subscription host.
/// </summary>
public sealed record CreateHostRequest
{
    /// <summary>
    /// Gets the host display name.
    /// </summary>
    [MaxLength(128)]
    public string? Name { get; init; }

    /// <summary>
    /// Gets the host address.
    /// </summary>
    [MaxLength(128)]
    public string? Address { get; init; }

    /// <summary>
    /// Gets the ISO 3166-1 alpha-2 country code.
    /// </summary>
    [MinLength(2)]
    [MaxLength(2)]
    public string? CountryAlpha2Code { get; init; }

    /// <summary>
    /// Gets the inbound identifier.
    /// </summary>
    public long InboundId { get; init; }

    /// <summary>
    /// Gets optional port override.
    /// </summary>
    [Range(1, 65535)]
    public int? Port { get; init; }

    /// <summary>
    /// Gets optional TLS server name override.
    /// </summary>
    [MaxLength(128)]
    public string? ServerName { get; init; }

    /// <summary>
    /// Gets optional HTTP host override.
    /// </summary>
    [MaxLength(128)]
    public string? Host { get; init; }

    /// <summary>
    /// Gets optional request path override.
    /// </summary>
    [MaxLength(256)]
    public string? Path { get; init; }

    /// <summary>
    /// Gets the security mode.
    /// </summary>
    public string? Security { get; init; }

    /// <summary>
    /// Gets optional ALPN values.
    /// </summary>
    public List<string>? Alpn { get; init; }

    /// <summary>
    /// Gets the TLS fingerprint.
    /// </summary>
    public string? Fingerprint { get; init; }

    /// <summary>
    /// Gets optional fragment template.
    /// </summary>
    [MaxLength(100)]
    public string? FragmentTemplate { get; init; }

    /// <summary>
    /// Gets optional noise template.
    /// </summary>
    [MaxLength(2000)]
    public string? NoiseTemplate { get; init; }

    /// <summary>
    /// Gets whether the host is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets whether mux is enabled.
    /// </summary>
    public bool IsMuxEnabled { get; init; }

    /// <summary>
    /// Gets whether SNI should be reused as host.
    /// </summary>
    public bool IsUseServerNameAsHost { get; init; }

    /// <summary>
    /// Gets whether generated user agents should be randomized.
    /// </summary>
    public bool IsRandomUseragent { get; init; }

    /// <summary>
    /// Gets whether this host may increase generated subscription limits.
    /// </summary>
    public bool AllowIncrease { get; init; }
}
