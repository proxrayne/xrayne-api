namespace Api.Responses;

/// <summary>
/// API-facing panel bootstrap settings.
/// </summary>
public sealed class PanelSettingsDto
{
    /// <summary>
    /// Gets or sets the IP address used by the panel listener.
    /// </summary>
    public string? BindIp { get; set; }

    /// <summary>
    /// Gets or sets the public panel domain.
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Gets or sets the panel listener port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the web base path when supported by the runtime.
    /// </summary>
    public string? PathBase { get; set; }

    /// <summary>
    /// Gets or sets the session lifetime in minutes when supported by the runtime.
    /// </summary>
    public int SessionLifetimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the public certificate path used by Kestrel.
    /// </summary>
    public string? CertPublicKeyPath { get; set; }

    /// <summary>
    /// Gets or sets the private certificate key path used by Kestrel.
    /// </summary>
    public string? CertPrivateKeyPath { get; set; }
}
