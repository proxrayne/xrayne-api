using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRayne.Cli.Output;
using XRayne.Cli.Services.Contracts;

namespace XRayne.Cli.Commands.Cert;

public sealed class CertRenewCommand : Command
{
    public CertRenewCommand(IServiceProvider serviceProvider)
        : base("renew", "Renew the installed HTTPS certificate and restart the API")
    {
        var forceOption = new Option<bool>("--force")
        {
            Description = "Force renewal even when the certificate is not near expiry."
        };

        Add(forceOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            return await ExecuteAsync(
                scope.ServiceProvider,
                parseResult.GetValue(forceOption),
                cancellationToken);
        });
    }

    private static async Task<int> ExecuteAsync(
        IServiceProvider serviceProvider,
        bool force,
        CancellationToken cancellationToken)
    {
        var console = serviceProvider.GetRequiredService<ICliConsole>();
        var logger = serviceProvider.GetRequiredService<ILogger<CertRenewCommand>>();
        var apiInstallationService = serviceProvider.GetRequiredService<IApiInstallationService>();
        var acmeCertificateService = serviceProvider.GetRequiredService<IAcmeCertificateService>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        try
        {
            apiInstallationService.EnsureInstalled();

            var certName = configuration["Certificate:CertName"];
            var mode = configuration["Certificate:Mode"];
            var identifier = configuration["Certificate:Identifier"];
            var email = configuration["Certificate:Email"];
            var staging = configuration.GetValue("Certificate:Staging", false);
            if (string.IsNullOrWhiteSpace(certName))
            {
                throw new InvalidOperationException("No installed XRayne certificate was found. Run 'xrayne cert install' first.");
            }

            if (string.IsNullOrWhiteSpace(mode) || string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException("Certificate metadata is incomplete. Run 'xrayne cert install' again.");
            }

            console.Header("XRayne certificate renewal");
            console.Value("Certificate name", certName);
            console.Value("Mode", mode);
            console.Value("Identifier", identifier);

            console.Success("Requesting renewed certificate with acme.sh.");
            var issueResult = await acmeCertificateService.RenewCertificateAsync(
                new AcmeCertificateRequest(
                    mode,
                    identifier,
                    email,
                    certName,
                    staging,
                    force),
                cancellationToken);

            console.Success("Restarting API container.");
            await apiInstallationService.RunDockerComposeAsync("up -d --force-recreate api", cancellationToken);

            console.Header("Certificate renewal completed");
            console.Value("Certificate", issueResult.FullChainPath);

            return 0;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Certificate renewal failed.");
            console.Error(exception.Message);

            return 1;
        }
    }

}
