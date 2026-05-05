using System.CommandLine;
using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRayne.Cli.Output;
using XRayne.Cli.Services;

namespace XRayne.Cli.Commands.Api;

public sealed class ApiUpdateCommand : Command
{
    private const string LatestVersion = "latest";
    private const string ImageName = "xrayne-api";
    private const string InstallDirectory = "/opt/xrayne";
    private const string EnvPath = "/opt/xrayne/.env";
    private const string ComposePath = "/opt/xrayne/docker-compose.yml";
    private const string ApiImageVariable = "XRAYNE_API_IMAGE";

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

        try
        {
            EnsureInstalled();

            var installedVersion = ReadInstalledVersion()
                ?? throw new InvalidOperationException($"'{ApiImageVariable}' was not found in '{EnvPath}'. Run 'xrayne api install' first.");
            var release = await gitHubReleaseService.ResolveReleaseAsync(LatestVersion, cancellationToken);
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

            var imageArchivePath = Path.Combine(InstallDirectory, asset.Name);
            console.Success($"Downloading {asset.Name} from {gitHubReleaseService.Repository} {release.TagName}.");
            await gitHubReleaseService.DownloadAssetAsync(asset.DownloadUrl, imageArchivePath, cancellationToken);

            var imageTarPath = Path.Combine(InstallDirectory, $"{ImageName}-{latestVersion}.tar");
            await DecompressGzipAsync(imageArchivePath, imageTarPath, cancellationToken);

            console.Success("Loading Docker image.");
            await shellService.RunAsync("docker", $"load -i \"{imageTarPath}\"", InstallDirectory, cancellationToken);

            UpdateEnvImage(latestVersion);

            console.Success("Restarting Docker Compose.");
            await apiInstallationService.RunDockerComposeAsync("up -d", cancellationToken);

            Console.WriteLine();
            Console.WriteLine("XRayne API update completed.");
            Console.WriteLine($"Previous API Version: {installedVersion}");
            Console.WriteLine($"Current API Version: {latestVersion}");
            Console.WriteLine($"Docker image: {ImageName}:{latestVersion}");

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
        if (!File.Exists(EnvPath))
        {
            throw new InvalidOperationException($"Environment file '{EnvPath}' was not found. Run 'xrayne api install' first.");
        }

        if (!File.Exists(ComposePath))
        {
            throw new InvalidOperationException($"Compose file '{ComposePath}' was not found. Run 'xrayne api install' first.");
        }
    }

    private static string? ReadInstalledVersion()
    {
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

    private static void UpdateEnvImage(string imageTag)
    {
        var lines = File.ReadAllLines(EnvPath);
        var imageLine = $"{ApiImageVariable}={ImageName}:{imageTag}";
        var updated = false;

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
            {
                continue;
            }

            var parts = line.Split('=', 2);
            if (parts.Length == 2 && string.Equals(parts[0].Trim(), ApiImageVariable, StringComparison.OrdinalIgnoreCase))
            {
                lines[index] = imageLine;
                updated = true;
                break;
            }
        }

        if (!updated)
        {
            lines = [.. lines, imageLine];
        }

        File.WriteAllLines(EnvPath, lines, Encoding.UTF8);
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
