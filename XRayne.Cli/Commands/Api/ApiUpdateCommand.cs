using System.CommandLine;
using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRayne.Cli.Output;
using XRayne.Cli.Services;
using XRayne.Cli.Values;
using XRayne.Infrastructure.Values;

namespace XRayne.Cli.Commands.Api;

public sealed class ApiUpdateCommand : Command
{
    public ApiUpdateCommand(IServiceProvider serviceProvider)
        : base("update", "Update installed XRayne API to the latest release")
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
        var logger = serviceProvider.GetRequiredService<ILogger<ApiUpdateCommand>>();
        var gitHubReleaseService = serviceProvider.GetRequiredService<IGitHubReleaseService>();
        var shellService = serviceProvider.GetRequiredService<IShellService>();
        var apiInstallationService = serviceProvider.GetRequiredService<IApiInstallationService>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        try
        {
            EnsureInstalled();

            var installedVersion = ExtractImageTag(configuration[CliDefaults.ApiImageVariable] ?? string.Empty)
                ?? throw new InvalidOperationException($"'{CliDefaults.ApiImageVariable}' was not found in '{PathProvider.Paths.EnvConfig}'. Run 'xrayne api install' first.");
            var release = await gitHubReleaseService.ResolveReleaseAsync(CliDefaults.LatestVersion, cancellationToken);
            var latestVersion = SanitizeDockerTag(release.TagName);

            Console.WriteLine($"Installed API Version: {installedVersion}");
            Console.WriteLine($"Latest API Version: {latestVersion}");

            if (string.Equals(installedVersion, latestVersion, StringComparison.Ordinal))
            {
                Console.WriteLine("Update Available: no");

                return 0;
            }

            Console.WriteLine("Update Available: yes");

            var assetName = $"xrayne-api-image-{latestVersion}.tar.gz";
            var asset = release.Assets.SingleOrDefault(item => string.Equals(item.Name, assetName, StringComparison.Ordinal));
            if (asset is null)
            {
                throw new InvalidOperationException($"Release asset '{assetName}' was not found in release '{release.TagName}'.");
            }

            var imageArchivePath = Path.Combine(PathProvider.Paths.DownloadsDirectory, asset.Name);
            console.Success($"Downloading {asset.Name} from {gitHubReleaseService.Repository} {release.TagName}.");
            await gitHubReleaseService.DownloadAssetAsync(asset.DownloadUrl, imageArchivePath, cancellationToken);

            var imageTarPath = Path.Combine(PathProvider.Paths.Root, $"{CliDefaults.ImageName}-{latestVersion}.tar");
            await DecompressGzipAsync(imageArchivePath, imageTarPath, cancellationToken);

            console.Success("Loading Docker image.");
            await shellService.RunAsync("docker", $"load -i \"{imageTarPath}\"", PathProvider.Paths.Root, cancellationToken);

            console.Success("Restarting Docker Compose.");
            apiInstallationService.EnsureInstalled();
            var environment = new Dictionary<string, string>(
                configuration.AsEnumerable()
                    .Where(item => !string.IsNullOrWhiteSpace(item.Value))
                    .Where(item => !item.Key.Contains(':', StringComparison.Ordinal))
                    .ToDictionary(item => item.Key, item => item.Value!, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase)
            {
                [CliDefaults.ApiImageVariable] = $"{CliDefaults.ImageName}:{latestVersion}"
            };
            await shellService.RunAsync("docker", "compose up -d", PathProvider.Paths.Root, environment, cancellationToken);

            console.Header("XRayne API update completed");
            console.Value("Previous API version", installedVersion);
            console.Value("Current API version", latestVersion);
            console.Value("Docker image", $"{CliDefaults.ImageName}:{latestVersion}");
            console.Value("Project path", PathProvider.Paths.Root);
            console.Value("Compose file", PathProvider.Paths.DockerCompose);
            console.Warning($"Static .env was not changed. Update '{CliDefaults.ApiImageVariable}' in '{PathProvider.Paths.EnvConfig}' manually to make this version permanent across future compose restarts.");

            return 0;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "API update failed.");
            console.Error(exception.Message);

            return 1;
        }
    }

    private static void EnsureInstalled()
    {
        if (!File.Exists(PathProvider.Paths.EnvConfig))
        {
            throw new InvalidOperationException($"Environment file '{PathProvider.Paths.EnvConfig}' was not found. Run 'xrayne api install' first.");
        }

        if (!File.Exists(PathProvider.Paths.DockerCompose))
        {
            throw new InvalidOperationException($"Compose file '{PathProvider.Paths.DockerCompose}' was not found. Run 'xrayne api install' first.");
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

    private static async Task DecompressGzipAsync(
        string archivePath,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        await using var input = File.OpenRead(archivePath);
        await using var gzip = new GZipStream(input, CompressionMode.Decompress);
        await using var output = File.Create(destinationPath);
        await gzip.CopyToAsync(output, cancellationToken);
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
