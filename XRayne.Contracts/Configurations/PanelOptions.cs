namespace XRayne.Contracts.Configurations;

public sealed class PanelOptions
{
    [RestartImpact(RestartImpact.FullRestart)]
    public string? BindIp { get; set; }

    [RestartImpact(RestartImpact.FullRestart)]
    public string? Domain { get; set; }

    [RestartImpact(RestartImpact.FullRestart)]
    public int Port { get; set; } = 5097;

    [RestartImpact(RestartImpact.FullRestart)]
    public string WebBasePath { get; set; } = "/";

    [RestartImpact(RestartImpact.FullRestart)]
    public int SessionLifetimeMinutes { get; set; } = 7200;

    [RestartImpact(RestartImpact.FullRestart)]
    public string? TrustedProxyCidrs { get; set; }

    [RestartImpact(RestartImpact.HotReload)]
    public string? CertificatesDirectory { get; set; }

    [RestartImpact(RestartImpact.HotReload)]
    public string? GeoResourcesDirectory { get; set; }

    [RestartImpact(RestartImpact.FullRestart)]
    public string? PanelCertPublicKeyPath { get; set; }

    [RestartImpact(RestartImpact.FullRestart)]
    public string? PanelCertPrivateKeyPath { get; set; }

    public PanelOptions Clone() => new()
    {
        BindIp = BindIp,
        Domain = Domain,
        Port = Port,
        WebBasePath = WebBasePath,
        SessionLifetimeMinutes = SessionLifetimeMinutes,
        TrustedProxyCidrs = TrustedProxyCidrs,
        CertificatesDirectory = CertificatesDirectory,
        GeoResourcesDirectory = GeoResourcesDirectory,
        PanelCertPublicKeyPath = PanelCertPublicKeyPath,
        PanelCertPrivateKeyPath = PanelCertPrivateKeyPath
    };
}
