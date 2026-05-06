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
        var gitHubReleaseService = serviceProvider.GetRequiredService<IGitHubReleaseService>();

        try
        {
            var apiStatus = await GetApiStatusAsync(apiInstallationService, cancellationToken);
            var apiPort = GetConfigurationValue(configuration, "API_PORT", CliDefaults.DefaultApiPort.ToString());
            var pathBase = NormalizePathBase(configuration["PathBase"]);
            var serverIp = GetServerIpAddress();
            var cliVersion = GetVersion();
            var apiVersion = ExtractImageTag(configuration[CliDefaults.ApiImageVariable] ?? string.Empty);
            var updateStatus = await GetUpdateStatusAsync(
                gitHubReleaseService,
                cliVersion,
                apiVersion,
                cancellationToken);

            console.Header("XRayne CLI information");
            console.Value("CLI version", cliVersion);
            console.Value("CLI directory", PathProvider.GetCliDirectory()?.FullName ?? AppContext.BaseDirectory);
            console.Value("Project directory", PathProvider.GetProjectDirectory());

            console.Section("API");
            console.Value("Status", apiStatus);
            console.Value("Server IP", serverIp);
            console.Value("Panel URL", $"http://{serverIp}:{apiPort}{pathBase}/");
            console.Value("API URL", $"http://{serverIp}:{apiPort}{pathBase}/api");
            console.Value("Docker image", GetConfigurationValue(configuration, CliDefaults.ApiImageVariable, "(unknown)"));

            console.Section("Updates");
            console.Value("Latest release", updateStatus.LatestRelease);
            console.Value("CLI update", updateStatus.CliUpdate);
            console.Value("API update", updateStatus.ApiUpdate);

            console.Section("Project files");
            console.Value("Environment file", FormatPathState(PathProvider.Paths.EnvConfig));
            console.Value("Config file", FormatPathState(PathProvider.Paths.JsonConfig));
            console.Value("Compose file", FormatPathState(PathProvider.Paths.DockerCompose));
            console.Value("Logs directory", FormatPathState(PathProvider.Paths.LogsDirectory));
            console.Value("Xray directory", FormatPathState(PathProvider.Paths.XrayDirectory));
            console.Value("PostgreSQL data", FormatPathState(PathProvider.Paths.PostgresDirectory));

            return 0;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "CLI information lookup failed.");
            console.Error(exception.Message);

            return 1;
        }
    }

    private static async Task<UpdateStatus> GetUpdateStatusAsync(
        IGitHubReleaseService gitHubReleaseService,
        string cliVersion,
        string? apiVersion,
        CancellationToken cancellationToken)
    {
        try
        {
            var release = await gitHubReleaseService.ResolveReleaseAsync(CliDefaults.LatestVersion, cancellationToken);
            var latestApiVersion = SanitizeDockerTag(release.TagName);

            var cliUpdate = string.Equals(cliVersion, release.TagName, StringComparison.Ordinal)
                ? "not available"
                : $"available ({cliVersion} -> {release.TagName})";

            var apiUpdate = string.IsNullOrWhiteSpace(apiVersion)
                ? $"not installed (latest: {latestApiVersion})"
                : string.Equals(apiVersion, latestApiVersion, StringComparison.Ordinal)
                    ? "not available"
                    : $"available ({apiVersion} -> {latestApiVersion})";

            return new UpdateStatus(release.TagName, cliUpdate, apiUpdate);
        }
        catch (Exception exception)
        {
            var message = exception.GetBaseException().Message;

            return new UpdateStatus(
                $"unavailable ({message})",
                "unknown",
                string.IsNullOrWhiteSpace(apiVersion) ? "not installed" : "unknown");
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

    private static string? ExtractImageTag(string image)
    {
        const string imagePrefix = CliDefaults.ImageName + ":";

        if (image.StartsWith(imagePrefix, StringComparison.Ordinal))
        {
            return image[imagePrefix.Length..];
        }

        var tagSeparatorIndex = image.LastIndexOf(':');

        return tagSeparatorIndex >= 0 && tagSeparatorIndex < image.Length - 1
            ? image[(tagSeparatorIndex + 1)..]
            : null;
    }

    private static string SanitizeDockerTag(string value)
    {
        var chars = value.Select(character =>
            char.IsAsciiLetterOrDigit(character) || character is '_' or '.' or '-'
                ? character
                : '-').ToArray();
        var tag = new string(chars).Trim('-');

        return string.IsNullOrWhiteSpace(tag) ? "latest" : tag;
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

    private sealed record UpdateStatus(
        string LatestRelease,
        string CliUpdate,
        string ApiUpdate);
}
