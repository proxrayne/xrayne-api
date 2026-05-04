using System.CommandLine;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRayne.Cli.Output;

namespace XRayne.Cli.Commands.Api;

public sealed class ApiInstallCommand : Command
{
    private const string LatestVersion = "latest";
    private const string ImageName = "xrayne-api";
    private const string Repository = "VanyaKrotov/xrayne";
    private const string InstallDirectory = "/opt/xrayne";
    private const string DefaultDataFolder = "/usr/shared/xrayne";
    private const int DefaultApiPort = 5000;
    private const string PostgresUser = "postgres";
    private const string PostgresDatabase = "xrayne";

    public ApiInstallCommand(IServiceProvider serviceProvider)
        : base("install", "Download and install XRayne API Docker image")
    {
        var versionOption = new Option<string>("--version")
        {
            Description = "GitHub release tag to install, or 'latest'.",
            DefaultValueFactory = _ => LatestVersion
        };

        Add(versionOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            return await ExecuteAsync(
                scope.ServiceProvider,
                parseResult.GetValue(versionOption) ?? LatestVersion,
                cancellationToken);
        });
    }

    private static async Task<int> ExecuteAsync(
        IServiceProvider serviceProvider,
        string version,
        CancellationToken cancellationToken)
    {
        var console = serviceProvider.GetRequiredService<ICliConsole>();
        var logger = serviceProvider.GetRequiredService<ILogger<ApiInstallCommand>>();

        try
        {
            var options = ReadInstallOptions();
            var release = await ResolveReleaseAsync(version, cancellationToken);
            var imageTag = SanitizeDockerTag(release.TagName);
            var assetName = $"xrayne-api-image-{imageTag}.tar.gz";
            var asset = release.Assets.SingleOrDefault(item => string.Equals(item.Name, assetName, StringComparison.Ordinal));
            if (asset is null)
            {
                throw new InvalidOperationException($"Release asset '{assetName}' was not found in release '{release.TagName}'.");
            }

            Directory.CreateDirectory(InstallDirectory);
            Directory.CreateDirectory(options.DataFolder);

            var imageArchivePath = Path.Combine(InstallDirectory, asset.Name);
            console.Success($"Downloading {asset.Name} from {Repository} {release.TagName}.");
            await DownloadFileAsync(asset.DownloadUrl, imageArchivePath, cancellationToken);

            var imageTarPath = Path.Combine(InstallDirectory, $"{ImageName}-{imageTag}.tar");
            await DecompressGzipAsync(imageArchivePath, imageTarPath, cancellationToken);

            console.Success("Loading Docker image.");
            await RunProcessAsync("docker", $"load -i \"{imageTarPath}\"", InstallDirectory, cancellationToken);

            var envPath = Path.Combine(InstallDirectory, ".env");
            WriteEnvFile(envPath, imageTag, options);

            var composePath = Path.Combine(InstallDirectory, "docker-compose.yml");
            await File.WriteAllTextAsync(composePath, CreateDockerCompose(imageTag), Encoding.UTF8, cancellationToken);

            console.Success($"API installation files are ready in '{InstallDirectory}'.");
            console.Success("Starting Docker Compose.");
            await RunProcessAsync("docker", "compose up -d", InstallDirectory, cancellationToken);

            PrintInstallSummary(release.TagName, imageTag, options);

            return 0;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "API installation failed.");
            console.Error(exception.Message);

            return 1;
        }
    }

    private static InstallOptions ReadInstallOptions()
    {
        var apiPort = ReadInt(
            $"API port [{DefaultApiPort}]: ",
            DefaultApiPort,
            value => value is >= 1 and <= 65535,
            "Port must be between 1 and 65535.");

        Console.Write($"PostgreSQL user is '{PostgresUser}'. Enter PostgreSQL password or leave empty to generate one: ");
        var postgresPassword = Console.ReadLine();
        var isGeneratedPassword = string.IsNullOrWhiteSpace(postgresPassword);
        if (isGeneratedPassword)
        {
            postgresPassword = GeneratePassword();
        }

        Console.Write($"Data folder [{DefaultDataFolder}/]: ");
        var dataFolder = NormalizeDirectory(Console.ReadLine(), DefaultDataFolder);

        Console.Write("API prefix, for example 'hidden-panel' (empty for no prefix): ");
        var apiPrefix = NormalizePrefix(Console.ReadLine());

        return new InstallOptions(
            apiPort,
            postgresPassword!,
            isGeneratedPassword,
            dataFolder,
            apiPrefix);
    }

    private static int ReadInt(
        string prompt,
        int defaultValue,
        Func<int, bool> validate,
        string errorMessage)
    {
        while (true)
        {
            Console.Write(prompt);
            var raw = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return defaultValue;
            }

            if (int.TryParse(raw, out var value) && validate(value))
            {
                return value;
            }

            Console.WriteLine(errorMessage);
        }
    }

    private static string NormalizeDirectory(string? value, string defaultValue)
    {
        var directory = string.IsNullOrWhiteSpace(value)
            ? defaultValue
            : value.Trim();

        return directory.TrimEnd('/', '\\');
    }

    private static string NormalizePrefix(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var prefix = value.Trim().Trim('/');
        if (prefix.Length == 0)
        {
            return string.Empty;
        }

        if (prefix.Any(character => !(char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.')))
        {
            throw new InvalidOperationException("API prefix can contain only letters, digits, '-', '_' and '.'.");
        }

        return $"/{prefix}";
    }

    private static string GeneratePassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%^&*";

        return RandomNumberGenerator.GetString(chars, 16);
    }

    private static async Task<GitHubRelease> ResolveReleaseAsync(
        string version,
        CancellationToken cancellationToken)
    {
        var url = string.Equals(version, LatestVersion, StringComparison.OrdinalIgnoreCase)
            ? $"https://api.github.com/repos/{Repository}/releases/latest"
            : $"https://api.github.com/repos/{Repository}/releases/tags/{Uri.EscapeDataString(version)}";

        using var client = CreateGitHubClient();
        using var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        var tagName = root.GetProperty("tag_name").GetString()
            ?? throw new InvalidOperationException("GitHub release response does not contain tag_name.");
        var assets = root.GetProperty("assets")
            .EnumerateArray()
            .Select(item => new GitHubAsset(
                item.GetProperty("name").GetString() ?? string.Empty,
                item.GetProperty("url").GetString() ?? string.Empty))
            .Where(item => !string.IsNullOrWhiteSpace(item.Name) && !string.IsNullOrWhiteSpace(item.DownloadUrl))
            .ToArray();

        return new GitHubRelease(tagName, assets);
    }

    private static HttpClient CreateGitHubClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("xrayne-cli", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        return client;
    }

    private static async Task DownloadFileAsync(
        string assetUrl,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        using var client = CreateGitHubClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

        using var response = await client.GetAsync(assetUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var output = File.Create(destinationPath);
        await input.CopyToAsync(output, cancellationToken);
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

    private static void WriteEnvFile(string envPath, string imageTag, InstallOptions options)
    {
        var values = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["XRAYNE_API_IMAGE"] = $"{ImageName}:{imageTag}",
            ["XRAYNE_API_PORT"] = options.ApiPort.ToString(),
            ["XRAYNE_API_PREFIX"] = options.ApiPrefix,
            ["XRAYNE_DATA_FOLDER"] = options.DataFolder,
            ["POSTGRES_DB"] = PostgresDatabase,
            ["POSTGRES_USER"] = PostgresUser,
            ["POSTGRES_PASSWORD"] = options.PostgresPassword,
            ["POSTGRES_PORT"] = "5432"
        };

        File.WriteAllLines(
            envPath,
            values.Select(item => $"{item.Key}={item.Value}"),
            Encoding.UTF8);
    }

    private static string CreateDockerCompose(string imageTag)
    {
        return $$"""
               services:
                 api:
                   image: ${XRAYNE_API_IMAGE:-{{ImageName}}:{{imageTag}}}
                   container_name: xrayne-api
                   env_file:
                     - /opt/xrayne/.env
                   environment:
                     ASPNETCORE_URLS: "http://+:8080"
                     PathBase: "${XRAYNE_API_PREFIX}"
                     ConnectionStrings__Default: "Host=postgres;Port=5432;Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD};Database=${POSTGRES_DB}"
                   ports:
                     - "${XRAYNE_API_PORT:-5000}:8080"
                   volumes:
                     - ${XRAYNE_DATA_FOLDER:-/usr/shared/xrayne}:/app/shared
                     - ./data/api/logs:/app/logs
                   depends_on:
                     postgres:
                       condition: service_healthy
                   restart: unless-stopped

                 postgres:
                   image: postgres:16-alpine
                   container_name: xrayne-postgres
                   env_file:
                     - /opt/xrayne/.env
                   environment:
                     POSTGRES_DB: ${POSTGRES_DB}
                     POSTGRES_USER: ${POSTGRES_USER}
                     POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
                   ports:
                     - "${POSTGRES_PORT:-5432}:5432"
                   volumes:
                     - ./data/postgres:/var/lib/postgresql/data
                   healthcheck:
                     test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB}"]
                     interval: 10s
                     timeout: 5s
                     retries: 5
                   restart: unless-stopped
               """;
    }

    private static void PrintInstallSummary(string releaseTag, string imageTag, InstallOptions options)
    {
        var panelUrl = $"http://localhost:{options.ApiPort}{options.ApiPrefix}/";
        var apiUrl = $"http://localhost:{options.ApiPort}{options.ApiPrefix}/api";

        Console.WriteLine();
        Console.WriteLine("XRayne API installation completed.");
        Console.WriteLine("==================================");
        Console.WriteLine($"Release: {releaseTag}");
        Console.WriteLine($"Docker image: {ImageName}:{imageTag}");
        Console.WriteLine($"Installation directory: {InstallDirectory}");
        Console.WriteLine($"Environment file: {Path.Combine(InstallDirectory, ".env")}");
        Console.WriteLine($"Compose file: {Path.Combine(InstallDirectory, "docker-compose.yml")}");
        Console.WriteLine();
        Console.WriteLine("Panel");
        Console.WriteLine($"  URL: {panelUrl}");
        Console.WriteLine($"  API URL: {apiUrl}");
        Console.WriteLine($"  Prefix: {(string.IsNullOrWhiteSpace(options.ApiPrefix) ? "(none)" : options.ApiPrefix)}");
        Console.WriteLine();
        Console.WriteLine("PostgreSQL");
        Console.WriteLine("  Host from API container: postgres");
        Console.WriteLine("  Port from API container: 5432");
        Console.WriteLine("  Host port: 5432");
        Console.WriteLine($"  Database: {PostgresDatabase}");
        Console.WriteLine($"  Username: {PostgresUser}");
        Console.WriteLine($"  Password: {options.PostgresPassword}");
        Console.WriteLine($"  Password generated: {(options.IsGeneratedPassword ? "yes" : "no")}");
        Console.WriteLine($"  Connection string: Host=localhost;Port=5432;Username={PostgresUser};Password={options.PostgresPassword};Database={PostgresDatabase}");
        Console.WriteLine();
        Console.WriteLine("Data");
        Console.WriteLine($"  Host data folder: {options.DataFolder}");
        Console.WriteLine("  Container data folder: /app/shared");
        Console.WriteLine();
        Console.WriteLine("Next useful commands");
        Console.WriteLine($"  cd {InstallDirectory}");
        Console.WriteLine("  docker compose ps");
        Console.WriteLine("  docker compose logs -f api");
        Console.WriteLine("  docker compose logs -f postgres");
    }

    private static async Task RunProcessAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(fileName, arguments)
            {
                WorkingDirectory = workingDirectory,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            }
        };

        process.Start();

        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode == 0)
        {
            return;
        }

        var output = string.Join(Environment.NewLine, await stdout, await stderr).Trim();
        throw new InvalidOperationException($"{fileName} {arguments} failed with exit code {process.ExitCode}.{Environment.NewLine}{output}");
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

    private sealed record InstallOptions(
        int ApiPort,
        string PostgresPassword,
        bool IsGeneratedPassword,
        string DataFolder,
        string ApiPrefix);

    private sealed record GitHubRelease(string TagName, IReadOnlyCollection<GitHubAsset> Assets);

    private sealed record GitHubAsset(string Name, string DownloadUrl);
}
