using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using XRayne.Repositories;
using PanelSettingsEntity = XRayne.Repositories.Entities.PanelSettings;

namespace XRayne.Api.Auth;

public sealed record PanelStartupSettings(
    string? BindIp,
    int Port,
    string? CertPublicKeyPath,
    string? CertPrivateKeyPath,
    string? Domain,
    string WebBasePath,
    int SessionLifetimeMinutes,
    string? TrustedProxyCidrs);

// Читает FullRestart-поля из БД до построения DI — Kestrel bind, CORS, ForwardedHeaders
// и JWT lifetime фиксируются на старте. HotReload-поля идут через IPanelSettingsAccessor.
public static class PanelStartupReader
{
    public static PanelStartupSettings? TryRead(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            using var context = new AppDbContext(options);
            var row = context.PanelSettings
                .AsNoTracking()
                .FirstOrDefault(s => s.Id == PanelSettingsEntity.SingletonId);

            if (row is null)
            {
                return null;
            }

            return new PanelStartupSettings(
                BindIp: row.BindIp,
                Port: row.Port,
                CertPublicKeyPath: row.PanelCertPublicKeyPath,
                CertPrivateKeyPath: row.PanelCertPrivateKeyPath,
                Domain: row.Domain,
                WebBasePath: row.WebBasePath,
                SessionLifetimeMinutes: row.SessionLifetimeMinutes,
                TrustedProxyCidrs: row.TrustedProxyCidrs);
        }
        catch (NpgsqlException ex)
        {
            // PostgresException наследник NpgsqlException — попадает сюда же.
            Log.Warning(ex, "panel_settings unreadable on startup; falling back to defaults.");
            return null;
        }
        catch (SocketException ex)
        {
            Log.Warning(ex, "panel_settings host unreachable on startup; falling back to defaults.");
            return null;
        }
        catch (InvalidOperationException ex)
        {
            Log.Warning(ex, "panel_settings connection setup failed; falling back to defaults.");
            return null;
        }
    }

    public static bool ShouldOverrideKestrel(PanelStartupSettings settings) =>
        !string.IsNullOrWhiteSpace(settings.BindIp)
        || settings.Port != 5097
        || !string.IsNullOrWhiteSpace(settings.CertPublicKeyPath)
        || !string.IsNullOrWhiteSpace(settings.CertPrivateKeyPath);

    public static void ApplyKestrel(KestrelServerOptions kestrel, PanelStartupSettings settings, Microsoft.Extensions.Logging.ILogger logger)
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
