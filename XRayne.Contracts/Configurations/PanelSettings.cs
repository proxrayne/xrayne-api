using Microsoft.Extensions.Configuration;

namespace XRayne.Contracts.Configurations;

public sealed class PanelSettings
{
    public string? BindIp { get; set; }

    public string? Domain { get; set; }

    public required int Port { get; set; }

    public string? PathBase { get; set; }

    public int SessionLifetimeMinutes { get; set; }

    public string? CertPublicKeyPath { get; set; }

    public string? CertPrivateKeyPath { get; set; }

    public PanelSettings Clone() => new()
    {
        Port = Port,
        BindIp = BindIp,
        Domain = Domain,
        PathBase = PathBase,
        SessionLifetimeMinutes = SessionLifetimeMinutes,
        CertPublicKeyPath = CertPublicKeyPath,
        CertPrivateKeyPath = CertPrivateKeyPath
    };

    public static PanelSettings Parse(IConfiguration configuration)
    {
        var port = configuration.GetValue<int>("API_PORT", 5097);
        var bindIp = configuration.GetValue<string>("BindIp");
        var domain = configuration.GetValue<string>("Domain");
        var pathBase = configuration.GetValue<string>("PathBase");
        var sessionLifetimeMinutes = configuration.GetValue<int>("Jwt:AccessTokenLifetimeMinutes", 7200);
        var panelCertPrivateKeyPath = configuration.GetValue<string>("Cert:PrivateKeyPath");
        var panelCertPublicKeyPath = configuration.GetValue<string>("Cert:PublicKeyPath");

        return new()
        {
            Port = port,
            BindIp = bindIp,
            Domain = domain,
            PathBase = pathBase,
            SessionLifetimeMinutes = sessionLifetimeMinutes,
            CertPrivateKeyPath = panelCertPrivateKeyPath,
            CertPublicKeyPath = panelCertPublicKeyPath,
        };
    }
}
