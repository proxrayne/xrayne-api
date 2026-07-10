using System.Text.Json.Serialization;

namespace Contracts.Enums;

/// <summary>
/// Defines the certificate target type used for remote node HTTPS setup.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CertificateMode
{
    /// <summary>
    /// Certificate is issued for a DNS domain name.
    /// </summary>
    [JsonStringEnumMemberName("domain")]
    Domain,

    /// <summary>
    /// Certificate is issued for a public IP address.
    /// </summary>
    [JsonStringEnumMemberName("ip")]
    Ip
}
