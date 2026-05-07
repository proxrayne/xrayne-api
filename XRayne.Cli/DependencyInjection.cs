using Microsoft.Extensions.DependencyInjection;
using XRayne.Cli.Commands;
using XRayne.Cli.Commands.Api;
using XRayne.Cli.Commands.Admin;
using XRayne.Cli.Commands.Cert;
using XRayne.Cli.Commands.Xray;
using XRayne.Cli.Output;
using XRayne.Cli.Services;
using XRayne.Cli.Services.Contracts;

namespace XRayne.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCliActions(this IServiceCollection services)
    {
        services.AddSingleton<ICliConsole, CliConsole>();
        services.AddScoped<IShellService, ShellService>();
        services.AddScoped<IGitHubReleaseService, GitHubReleaseService>();
        services.AddScoped<IApiInstallationService, ApiInstallationService>();
        services.AddScoped<IAcmeCertificateService, AcmeCertificateService>();
        services.AddScoped<IDockerComposeFileService, DockerComposeFileService>();

        services.AddSingleton<RootCommandFactory>();
        services.AddSingleton<VersionCommand>();
        services.AddSingleton<UpdateCommand>();
        services.AddSingleton<InfoCommand>();
        services.AddSingleton<ApiCommand>();
        services.AddSingleton<ApiInstallCommand>();
        services.AddSingleton<ApiVersionCommand>();
        services.AddSingleton<ApiStatusCommand>();
        services.AddSingleton<ApiStopCommand>();
        services.AddSingleton<ApiStartCommand>();
        services.AddSingleton<ApiRestartCommand>();
        services.AddSingleton<CertCommand>();
        services.AddSingleton<CertInstallCommand>();
        services.AddSingleton<CertRenewCommand>();
        services.AddSingleton<CertStatusCommand>();
        services.AddSingleton<XrayCommand>();
        services.AddSingleton<XrayStartCommand>();
        services.AddSingleton<AdminCommand>();
        services.AddSingleton<AdminCreateCommand>();

        return services;
    }
}
