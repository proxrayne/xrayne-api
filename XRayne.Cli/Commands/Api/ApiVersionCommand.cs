using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRayne.Cli.Output;
using XRayne.Cli.Values;
using XRayne.Infrastructure.GitHub;

namespace XRayne.Cli.Commands.Api;

public sealed class ApiVersionCommand : Command
{
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
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var repository = new GitHubRepository(CliDefaults.XRayneRepositoryUrl);

        try
        {
            var installedVersion = ExtractImageTag(configuration[CliDefaults.ApiImageVariable] ?? string.Empty);
            var release = await repository.GetReleaseAsync(CliDefaults.LatestVersion, cancellationToken);
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
}
