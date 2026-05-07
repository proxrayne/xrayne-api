using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRayne.Cli.Helpers;
using XRayne.Cli.Output;

namespace XRayne.Cli.Commands.Cert;

public sealed class CertStatusCommand : Command
{
    public CertStatusCommand(IServiceProvider serviceProvider)
        : base("status", "Print installed HTTPS certificate information")
    {
        SetAction(async (_, cancellationToken) =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            return Execute(scope.ServiceProvider);
        });
    }

    private static int Execute(IServiceProvider serviceProvider)
    {
        var console = serviceProvider.GetRequiredService<ICliConsole>();
        var logger = serviceProvider.GetRequiredService<ILogger<CertStatusCommand>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        try
        {
            var certName = configuration["Certificate:CertName"];
            if (string.IsNullOrWhiteSpace(certName))
            {
                console.Header("XRayne certificate status");
                console.Value("Status", "not installed");

                return 0;
            }

            var fullChainPath = configuration["Certificate:HostFullChainPath"]
                ?? CertificateCommandHelper.GetHostFullChainPath(certName);
            var privateKeyPath = configuration["Certificate:HostPrivateKeyPath"]
                ?? CertificateCommandHelper.GetHostPrivateKeyPath(certName);

            console.Header("XRayne certificate status");
            console.Value("Status", File.Exists(fullChainPath) && File.Exists(privateKeyPath) ? "installed" : "missing files");
            console.Value("Mode", configuration["Certificate:Mode"] ?? "(unknown)");
            console.Value("Identifier", configuration["Certificate:Identifier"] ?? "(unknown)");
            console.Value("ACME client", configuration["Certificate:AcmeClient"] ?? "(unknown)");
            console.Value("Issuer", configuration["Certificate:Issuer"] ?? "(unknown)");
            console.Value("Cert profile", configuration["Certificate:CertProfile"] ?? "(default)");
            console.Value("Auto renew", configuration.GetValue("Certificate:AutoRenew", false) ? "enabled" : "disabled");
            console.Value("Certificate name", certName);
            console.Value("Certificate", FormatPathState(fullChainPath));
            console.Value("Private key", FormatPathState(privateKeyPath));
            console.Value("HTTPS endpoint", configuration["Kestrel:Endpoints:Https:Url"] ?? "(not configured)");

            return 0;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Certificate status lookup failed.");
            console.Error(exception.Message);

            return 1;
        }
    }

    private static string FormatPathState(string path)
    {
        return File.Exists(path) || Directory.Exists(path)
            ? path
            : $"{path} (missing)";
    }
}
