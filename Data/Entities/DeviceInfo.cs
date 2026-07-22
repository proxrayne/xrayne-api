namespace Data.Entities;

/// <summary>
/// Stores optional device metadata reported by a connection client.
/// </summary>
public sealed class DeviceInfo
{
    /// <summary>
    /// Gets or sets the optional hardware identifier reported by the client.
    /// </summary>
    public string? HWID { get; set; }

    /// <summary>
    /// Gets or sets the optional operating system name reported by the client.
    /// </summary>
    public string? OS { get; set; }

    /// <summary>
    /// Gets or sets the optional device model reported by the client.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets the optional application version reported by the client.
    /// </summary>
    public string? AppVersion { get; set; }
}
