using System.Text.Json.Serialization;

namespace Contracts.Enums;

/// <summary>
/// Defines how a connection device should be verified.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeviceVerificationMethod
{
    /// <summary>
    /// Do not verify the connection device.
    /// </summary>
    [JsonStringEnumMemberName("none")]
    None,

    /// <summary>
    /// Verify the connection device by the User-Agent header.
    /// </summary>
    [JsonStringEnumMemberName("user_agent")]
    UserAgent,

    /// <summary>
    /// Verify the connection device by reported device information when available.
    /// </summary>
    [JsonStringEnumMemberName("device_info")]
    DeviceInfo,

    /// <summary>
    /// Verify the connection device by all available device properties.
    /// </summary>
    [JsonStringEnumMemberName("combined")]
    Combined
}
