using System.Text.Json.Serialization;

namespace Contracts.Enums;

/// <summary>
/// Defines host-level security mode overrides.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HostSecurity
{
    /// <summary>
    /// Do not apply transport security.
    /// </summary>
    [JsonStringEnumMemberName("none")]
    None,

    /// <summary>
    /// Use the security mode configured by the inbound.
    /// </summary>
    [JsonStringEnumMemberName("inbound-default")]
    InboundDefault,

    /// <summary>
    /// Use TLS for the host.
    /// </summary>
    [JsonStringEnumMemberName("tls")]
    TLS,
}
