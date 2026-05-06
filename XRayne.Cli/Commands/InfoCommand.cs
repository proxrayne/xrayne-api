using System.CommandLine;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRayne.Cli.Output;
using XRayne.Cli.Services;
using XRayne.Cli.Values;
using XRayne.Infrastructure.Values;

namespace XRayne.Cli.Commands;

public sealed class InfoCommand : Command
{
    public InfoCommand(IServiceProvider serviceProvider)
        : base("info", "Print XRayne CLI, project, and API runtime information")
    {
        SetAction(async (_, cancellationToken) =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            return await ExecuteAsync(scope.ServiceProvider, cancellationToken);
        });
    }

    private static async Task<int> ExecuteAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var console = serviceProvider.GetRequiredService<ICliConsole>();
        var logger = serviceProvider.GetRequiredService<ILogger<InfoCommand>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var apiInstallationService = serviceProvider.GetRequiredService<IApiInstallationService>();

        try
        {
            var apiStatus = await GetApiStatusAsync(apiInstallationService, cancellationToken);
            var apiPort = GetConfigurationValue(configuration, "API_PORT", CliDefaults.DefaultApiPort.ToString());
            var pathBase = NormalizePathBase(configuration["PathBase"]);
            var serverIp = GetServerIpAddress();

            console.Header("XRayne CLI information");
            console.Value("CLI version", GetVersion());
            console.Value("CLI directory", PathProvider.GetCliDirectory()?.FullName ?? AppContext.BaseDirectory);
            console.Value("Project directory", PathProvider.GetProjectDirectory());

            console.Section("API");
            console.Value("Status", apiStatus);
            console.Value("Server IP", serverIp);
            console.Value("Panel URL", $"http://{serverIp}:{apiPort}{pathBase}/");
            console.Value("API URL", $"http://{serverIp}:{apiPort}{pathBase}/api");
            console.Value("Docker image", GetConfigurationValue(configuration, CliDefaults.ApiImageVariable, "(unknown)"));

            console.Section("Project files");
            console.Value("Environment file", FormatPathState(PathProvider.Paths.EnvConfig));
            console.Value("Config file", FormatPathState(PathProvider.Paths.JsonConfig));
            console.Value("Compose file", FormatPathState(PathProvider.Paths.DockerCompose));
            console.Value("Logs directory", FormatPathState(PathProvider.Paths.LogsDirectory));
            console.Value("Xray directory", FormatPathState(PathProvider.Paths.XrayDirectory));
            console.Value("PostgreSQL data", FormatPathState(PathProvider.Paths.PostgresDirectory));

            return 0;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "CLI information lookup failed.");
            console.Error(exception.Message);

            return 1;
        }
    }

    private static async Task<string> GetApiStatusAsync(
        IApiInstallationService apiInstallationService,
        CancellationToken cancellationToken)
    {
        try
        {
            return await apiInstallationService.IsApiRunningAsync(cancellationToken)
                ? "running"
                : "stopped";
        }
        catch
        {
            return "not installed";
        }
    }

    private static string GetVersion()
    {
        var assembly = typeof(InfoCommand).Assembly;
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            return informationalVersion.Split('+', 2)[0];
        }

        return assembly.GetName().Version?.ToString() ?? "unknown";
    }

    private static string GetConfigurationValue(
        IConfiguration configuration,
        string key,
        string fallback)
    {
        var value = configuration[key];

        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static string NormalizePathBase(string? pathBase)
    {
        if (string.IsNullOrWhiteSpace(pathBase))
        {
            return string.Empty;
        }

        return pathBase.StartsWith("/", StringComparison.Ordinal)
            ? pathBase.TrimEnd('/')
            : $"/{pathBase.TrimEnd('/')}";
    }

    private static string FormatPathState(string path)
    {
        return File.Exists(path) || Directory.Exists(path)
            ? path
            : $"{path} (missing)";
    }

    private static string GetServerIpAddress()
    {
        var address = NetworkInterface.GetAllNetworkInterfaces()
            .Where(item => item.OperationalStatus == OperationalStatus.Up)
            .Where(item => item.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .SelectMany(item => item.GetIPProperties().UnicastAddresses)
            .Select(item => item.Address)
            .FirstOrDefault(IsUsableIPv4Address);

        return address?.ToString() ?? IPAddress.Loopback.ToString();
    }

    private static bool IsUsableIPv4Address(IPAddress address)
    {
        return address.AddressFamily == AddressFamily.InterNetwork
            && !IPAddress.IsLoopback(address);
    }
}
