namespace Api.Responses;

public sealed class PanelSettingsResponse
{
    public required PanelSettingsDto Settings { get; init; }

    public bool PendingRestart { get; set; }
}


public sealed class PanelSettingsDto
{
    public string? BindIp { get; set; }

    public string? Domain { get; set; }

    public int Port { get; set; }

    public string? PathBase { get; set; }

    public int SessionLifetimeMinutes { get; set; }

    public string? CertPublicKeyPath { get; set; }

    public string? CertPrivateKeyPath { get; set; }

}