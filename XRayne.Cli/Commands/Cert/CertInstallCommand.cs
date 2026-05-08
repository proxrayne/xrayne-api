using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRayne.Cli.Helpers;
using XRayne.Cli.Output;
using XRayne.Cli.Services.Contracts;
using XRayne.Cli.Values;
using XRayne.Contracts.Values;
using XRayne.Infrastructure.Utilities;

namespace XRayne.Cli.Commands.Cert;

public sealed class CertInstallCommand : Command
{
    public CertInstallCommand(IServiceProvider serviceProvider)
        : base("install", "Issue and install an HTTPS certificate for the API container")
    {
        var domainOption = new Option<string?>("--domain")
        {
            Description = "Domain name to issue a certificate for."
        };
        var ipAddressOption = new Option<string?>("--ip-address")
        {
            Description = "Public IPv4 address to issue a short-lived certificate for. If omitted with no domain, the server public IP is used."
        };
        var emailOption = new Option<string>("--email")
        {
            Description = "ACME account email address."
        };
        var stagingOption = new Option<bool>("--staging")
        {
            Description = "Use the Let's Encrypt staging environment."
        };
        var forceOption = new Option<bool>("--force")
        {
            Description = "Force renewal even when a certificate already exists."
        };

        Add(domainOption);
        Add(ipAddressOption);
        Add(emailOption);
        Add(stagingOption);
        Add(forceOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            return await ExecuteAsync(
                scope.ServiceProvider,
                parseResult.GetValue(domainOption),
                parseResult.GetValue(ipAddressOption),
                parseResult.GetValue(emailOption) ?? string.Empty,
                parseResult.GetValue(stagingOption),
                parseResult.GetValue(forceOption),
                cancellationToken);
        });
    }

    private static async Task<int> ExecuteAsync(
        IServiceProvider serviceProvider,
        string? domain,
        string? ipAddress,
        string email,
        bool staging,
        bool force,
        CancellationToken cancellationToken)
    {
        var console = serviceProvider.GetRequiredService<ICliConsole>();
        var logger = serviceProvider.GetRequiredService<ILogger<CertInstallCommand>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var apiInstallationService = serviceProvider.GetRequiredService<IApiInstallationService>();
        var acmeCertificateService = serviceProvider.GetRequiredService<IAcmeCertificateService>();

        try
        {
            apiInstallationService.EnsureInstalled();
            ValidateEmail(email);

            var target = await ResolveCertificateTargetAsync(
                domain,
                ipAddress,
                cancellationToken);
            var certName = CertificateCommandHelper.BuildCertName(target.Mode, target.Identifier);

            Directory.CreateDirectory(PathProvider.Paths.CertificatesDirectory);
            Directory.CreateDirectory(PathProvider.Paths.LetsEncryptDirectory);

            var apiPort = GetApiPort(configuration);

            console.Header("XRayne certificate installation");
            console.Value("Mode", target.Mode);
            console.Value("Identifier", target.Identifier);
            console.Value("Certificate name", certName);
            console.Value("ACME client", "acme.sh");
            console.Value("Let's Encrypt storage", PathProvider.Paths.LetsEncryptDirectory);
            console.Value("HTTPS port", apiPort);

            if (target.Mode == "ip")
            {
                console.Warning("IP certificates use the Let's Encrypt shortlived profile and are renewed frequently.");
            }

            var request = new AcmeCertificateRequest(
                target.Mode,
                target.Identifier,
                email,
                certName,
                staging,
                force);

            console.Success("Requesting certificate with acme.sh.");
            var issueResult = await acmeCertificateService.IssueCertificateAsync(request, cancellationToken);

            console.Success("Enabling automatic certificate renewal.");
            await acmeCertificateService.EnableAutoRenewAsync(cancellationToken);

            await WriteCertificateConfigAsync(
                target.Mode,
                target.Identifier,
                email,
                certName,
                apiPort,
                staging,
                cancellationToken);

            console.Success("Restarting API with HTTPS configuration.");
            await apiInstallationService.RunDockerComposeAsync("up -d --force-recreate api", cancellationToken);

            console.Header("Certificate installed");
            console.Value("Mode", target.Mode);
            console.Value("Identifier", target.Identifier);
            console.Value("Certificate", issueResult.FullChainPath);
            console.Value("Private key", issueResult.PrivateKeyPath);
            console.Value("Container certificate", CertificateCommandHelper.GetContainerFullChainPath(certName));
            console.Value("Auto renew", "enabled");
            console.Value("HTTPS URL", $"https://{target.Identifier}:{apiPort}/");

            return 0;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Certificate installation failed.");
            console.Error(exception.Message);

            return 1;
        }
    }

    private static async Task WriteCertificateConfigAsync(
        string mode,
        string identifier,
        string email,
        string certName,
        string apiPort,
        bool staging,
        CancellationToken cancellationToken)
    {
        var fullChainPath = CertificateCommandHelper.GetContainerFullChainPath(certName);
        var privateKeyPath = CertificateCommandHelper.GetContainerPrivateKeyPath(certName);

        await JsonConfig.UpdateAsync(
            PathProvider.Paths.JsonConfig,
            config =>
            {
                JsonConfig.Remove(config, "Kestrel:Endpoints:Http");
                JsonConfig.Set(config, "Kestrel:Endpoints:Https:Url", $"https://+:{apiPort}");
                JsonConfig.Set(config, "Kestrel:Endpoints:Https:Certificate:Path", fullChainPath);
                JsonConfig.Set(config, "Kestrel:Endpoints:Https:Certificate:KeyPath", privateKeyPath);
                JsonConfig.Set(config, "Certificate:Mode", mode);
                JsonConfig.Set(config, "Certificate:Identifier", identifier);
                JsonConfig.Set(config, "Certificate:Email", email);
                JsonConfig.Set(config, "Certificate:Staging", staging);
                JsonConfig.Set(config, "Certificate:AcmeClient", "acme.sh");
                JsonConfig.Set(config, "Certificate:Issuer", "letsencrypt");
                JsonConfig.Set(config, "Certificate:CertProfile", mode == "ip" ? "shortlived" : string.Empty);
                JsonConfig.Set(config, "Certificate:AutoRenew", true);
                JsonConfig.Set(config, "Certificate:CertName", certName);
                JsonConfig.Set(config, "Certificate:FullChainPath", fullChainPath);
                JsonConfig.Set(config, "Certificate:PrivateKeyPath", privateKeyPath);
                JsonConfig.Set(config, "Certificate:HostFullChainPath", CertificateCommandHelper.GetHostFullChainPath(certName));
                JsonConfig.Set(config, "Certificate:HostPrivateKeyPath", CertificateCommandHelper.GetHostPrivateKeyPath(certName));
                JsonConfig.Set(config, "Certificate:UpdatedAtUtc", DateTimeOffset.UtcNow);
            },
            cancellationToken);
    }

    private static string GetApiPort(IConfiguration configuration)
    {
        var apiPort = configuration["API_PORT"];
        if (string.IsNullOrWhiteSpace(apiPort))
        {
            return CliDefaults.DefaultApiPort.ToString();
        }

        if (!int.TryParse(apiPort, out var port) || port is < 1 or > 65535)
        {
            throw new InvalidOperationException($"API_PORT '{apiPort}' is not a valid TCP port.");
        }

        return apiPort;
    }

    private static async Task<CertificateTarget> ResolveCertificateTargetAsync(
        string? domain,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(domain) && !string.IsNullOrWhiteSpace(ipAddress))
        {
            throw new InvalidOperationException("Use either --domain or --ip-address, not both.");
        }

        if (!string.IsNullOrWhiteSpace(domain))
        {
            return new CertificateTarget("domain", NormalizeDomain(domain));
        }

        var resolvedIpAddress = string.IsNullOrWhiteSpace(ipAddress)
            ? await NetworkAddress.GetPublicIpAddressAsync(cancellationToken)
            : ipAddress;

        return new CertificateTarget("ip", NetworkAddress.NormalizePublicIPv4Address(resolvedIpAddress));
    }

    private static string NormalizeDomain(string value)
    {
        var domain = value.Trim().TrimEnd('.').ToLowerInvariant();
        if (domain.Length == 0 || domain.Any(character => !(char.IsAsciiLetterOrDigit(character) || character is '-' or '.')))
        {
            throw new InvalidOperationException("Domain can contain only letters, digits, hyphens, and dots.");
        }

        return domain;
    }

    private static void ValidateEmail(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('@', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("--email is required and must contain a valid ACME account email address.");
        }
    }

    private sealed record CertificateTarget(
        string Mode,
        string Identifier);
}
