using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using XRayne.Contracts.Configurations;

namespace XRayne.Api.Auth;

public static class PanelStartupReader
{
    public static bool ShouldOverrideKestrel(PanelSettings settings) =>
        !string.IsNullOrWhiteSpace(settings.BindIp)
        || settings.Port != 5097
        || !string.IsNullOrWhiteSpace(settings.CertPublicKeyPath)
        || !string.IsNullOrWhiteSpace(settings.CertPrivateKeyPath);

    public static void ApplyKestrel(KestrelServerOptions kestrel, PanelSettings settings, ILogger logger)
    {
        var hasCert = !string.IsNullOrWhiteSpace(settings.CertPublicKeyPath)
            && !string.IsNullOrWhiteSpace(settings.CertPrivateKeyPath);

        if (hasCert && (!File.Exists(settings.CertPublicKeyPath) || !File.Exists(settings.CertPrivateKeyPath)))
        {
            logger.LogWarning(
                "Panel certificate paths configured but files missing ({Cert}, {Key}); falling back to HTTP.",
                settings.CertPublicKeyPath, settings.CertPrivateKeyPath);
            hasCert = false;
        }

        var bindAddress = ResolveBindAddress(settings.BindIp);

        kestrel.Listen(bindAddress, settings.Port, listen =>
        {
            if (hasCert)
            {
                listen.UseHttps(settings.CertPublicKeyPath!, settings.CertPrivateKeyPath!);
            }
        });
    }

    private static IPAddress ResolveBindAddress(string? bindIp)
    {
        if (string.IsNullOrWhiteSpace(bindIp))
        {
            return IPAddress.Any;
        }

        return IPAddress.TryParse(bindIp, out var addr) ? addr : IPAddress.Any;
    }
}
