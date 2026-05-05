using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRayne.Cli.Output;
using XRayne.Cli.Services;

namespace XRayne.Cli.Commands.Api;

public sealed class ApiVersionCommand : Command
{
    private const string LatestVersion = "latest";
    private const string ImageName = "xrayne-api";
    private const string EnvPath = "/opt/xrayne/.env";
    private const string ApiImageVariable = "XRAYNE_API_IMAGE";

    public ApiVersionCommand(IServiceProvider serviceProvider)
        : base("version", "Print installed XRayne API version and update status")
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
        var logger = serviceProvider.GetRequiredService<ILogger<ApiVersionCommand>>();
        var gitHubReleaseService = serviceProvider.GetRequiredService<IGitHubReleaseService>();

        try
        {
            var installedVersion = ReadInstalledVersion();
            var release = await gitHubReleaseService.ResolveReleaseAsync(LatestVersion, cancellationToken);
            var latestVersion = SanitizeDockerTag(release.TagName);

            if (string.IsNullOrWhiteSpace(installedVersion))
            {
                Console.WriteLine("API Version: not installed");
                Console.WriteLine($"Latest Version: {latestVersion}");
                Console.WriteLine("Update Available: no installed version found");

                return 0;
            }

            Console.WriteLine($"API Version: {installedVersion}");
            Console.WriteLine($"Latest Version: {latestVersion}");
            Console.WriteLine($"Update Available: {(!string.Equals(installedVersion, latestVersion, StringComparison.Ordinal) ? "yes" : "no")}");

            return 0;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "API version lookup failed.");
            console.Error(exception.Message);

            return 1;
        }
    }

    private static string? ReadInstalledVersion()
    {
        if (!File.Exists(EnvPath))
        {
            return null;
        }

        foreach (var line in File.ReadLines(EnvPath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            var parts = line.Split('=', 2);
            if (parts.Length != 2 || !string.Equals(parts[0].Trim(), ApiImageVariable, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return ExtractImageTag(parts[1].Trim().Trim('"', '\''));
        }

        return null;
    }

    private static string? ExtractImageTag(string image)
    {
        const string imagePrefix = ImageName + ":";

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
}
