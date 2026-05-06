using System.CommandLine;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRayne.Cli.Output;
using XRayne.Cli.Services;
using XRayne.Cli.Values;
using XRayne.Infrastructure.Services;
using XRayne.Infrastructure.Values;

namespace XRayne.Cli.Commands.Api;

public sealed class ApiInstallCommand : Command
{
    public ApiInstallCommand(IServiceProvider serviceProvider)
        : base("install", "Download and install XRayne API Docker image")
    {
        var versionOption = new Option<string>("--version")
        {
            Description = "GitHub release tag to install, or 'latest'.",
            DefaultValueFactory = _ => CliDefaults.LatestVersion
        };

        Add(versionOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            return await ExecuteAsync(
                scope.ServiceProvider,
                parseResult.GetValue(versionOption) ?? CliDefaults.LatestVersion,
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
        var gitHubReleaseService = serviceProvider.GetRequiredService<IGitHubReleaseService>();
        var shellService = serviceProvider.GetRequiredService<IShellService>();
        var apiInstallationService = serviceProvider.GetRequiredService<IApiInstallationService>();

        try
        {
            var options = ReadInstallOptions();

            var release = await gitHubReleaseService.ResolveReleaseAsync(version, cancellationToken);
            var imageTag = SanitizeDockerTag(release.TagName);
            var assetName = $"xrayne-api-image-{imageTag}.tar.gz";
            var asset = release.Assets.SingleOrDefault(item => string.Equals(item.Name, assetName, StringComparison.Ordinal));
            if (asset is null)
            {
                throw new InvalidOperationException($"Release asset '{assetName}' was not found in release '{release.TagName}'.");
            }

            Directory.CreateDirectory(options.Paths.LogsDirectory);
            Directory.CreateDirectory(options.Paths.PostgresDirectory);
            Directory.CreateDirectory(options.Paths.XrayDirectory);
            Directory.CreateDirectory(options.Paths.DownloadsDirectory);

            var imageArchivePath = Path.Combine(options.Paths.DownloadsDirectory, asset.Name);
            console.Success($"Downloading {asset.Name} from {gitHubReleaseService.Repository} {release.TagName}.");
            await gitHubReleaseService.DownloadAssetAsync(asset.DownloadUrl, imageArchivePath, cancellationToken);

            var imageTarPath = Path.Combine(options.Paths.Root, $"{CliDefaults.ImageName}-{imageTag}.tar");
            await DecompressGzipAsync(imageArchivePath, imageTarPath, cancellationToken);

            console.Success("Loading Docker image.");
            await shellService.RunAsync("docker", $"load -i \"{imageTarPath}\"", options.Paths.Root, cancellationToken);

            await WriteEnvFileAsync(imageTag, options, cancellationToken);
            await WriteConfigFileAsync(options, cancellationToken);

            await File.WriteAllTextAsync(options.Paths.DockerCompose, CreateDockerCompose(imageTag), Encoding.UTF8, cancellationToken);

            console.Success($"API installation files are ready in '{options.Paths.Root}'.");
            console.Success("Starting Docker Compose.");

            await apiInstallationService.RunDockerComposeAsync("up -d", cancellationToken);

            PrintInstallSummary(console, release.TagName, imageTag, options);

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
            $"API port [{CliDefaults.DefaultApiPort}]: ",
            CliDefaults.DefaultApiPort,
            value => value is >= 1 and <= 65535,
            "Port must be between 1 and 65535.");

        Console.Write($"PostgreSQL user is '{CliDefaults.PostgresUser}'. Enter PostgreSQL password or leave empty to generate one: ");
        var postgresPassword = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(postgresPassword))
        {
            postgresPassword = GeneratePassword();
        }

        Console.Write("API prefix, for example 'hidden-panel' (empty for no prefix): ");
        var apiPrefix = NormalizePrefix(Console.ReadLine());

        return new InstallOptions(PathProvider.GetProjectDirectory())
        {
            ApiPort = apiPort,
            ApiPrefix = apiPrefix,
            PostgresPassword = postgresPassword
        };
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

    private static async Task WriteEnvFileAsync(
        string imageTag,
        InstallOptions options,
        CancellationToken cancellationToken)
    {
        var values = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["API_PORT"] = options.ApiPort.ToString(),
            ["PROJECT_PATH"] = options.Paths.Root,
            ["API_IMAGE"] = $"{CliDefaults.ImageName}:{imageTag}",
            ["POSTGRES_DB"] = CliDefaults.PostgresDatabase,
            ["POSTGRES_HOST_API"] = "postgres",
            ["POSTGRES_USER"] = CliDefaults.PostgresUser,
            ["POSTGRES_PASSWORD"] = options.PostgresPassword,
            ["POSTGRES_CONTAINER_PORT"] = "5432",
            ["POSTGRES_PORT"] = "5432"
        };

        var lines = values.Select(item => $"{item.Key}={EscapeEnvironmentValue(item.Value)}");

        await File.WriteAllLinesAsync(options.Paths.EnvConfig, lines, Encoding.UTF8, cancellationToken);
    }

    private static async Task WriteConfigFileAsync(
        InstallOptions options,
        CancellationToken cancellationToken)
    {
        var config = new JsonConfigService(options.Paths.JsonConfig);

        config.Set("PathBase", options.ApiPrefix);

        await config.SaveAsync(cancellationToken);
    }

    private static string CreateDockerCompose(string imageTag)
    {
        return $$"""
               services:
                 api:
                   image: ${API_IMAGE:-{{CliDefaults.ImageName}}:{{imageTag}}}
                   container_name: xrayne-api
                   environment:
                     ASPNETCORE_URLS: "http://+:8080"
                     XRAYNE_CONFIG_FILE: "/app/config.json"
                     XRAYNE_ENV_FILE: "/app/.env"
                     ConnectionStrings__Default: "Host=${POSTGRES_HOST_API:-postgres};Port=${POSTGRES_CONTAINER_PORT:-5432};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD};Database=${POSTGRES_DB}"
                   ports:
                     - "${API_PORT:-5000}:8080"
                   volumes:
                     - ${PROJECT_PATH:-/opt/xrayne}/config.json:/app/config.json:ro
                     - ${PROJECT_PATH:-/opt/xrayne}/.env:/app/.env:ro
                     - ${PROJECT_PATH:-/opt/xrayne}:/app/shared
                     - ${PROJECT_PATH:-/opt/xrayne}/logs/api:/app/logs
                     - ${PROJECT_PATH:-/opt/xrayne}/xray:/app/xray
                   depends_on:
                     postgres:
                       condition: service_healthy
                   restart: unless-stopped

                 postgres:
                   image: postgres:16-alpine
                   container_name: xrayne-postgres
                   environment:
                     POSTGRES_DB: ${POSTGRES_DB}
                     POSTGRES_USER: ${POSTGRES_USER}
                     POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
                   ports:
                     - "${POSTGRES_PORT:-5432}:5432"
                   volumes:
                     - "${PROJECT_PATH:-/opt/xrayne}/postgres:/var/lib/postgresql/data"
                   healthcheck:
                     test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB}"]
                     interval: 10s
                     timeout: 5s
                     retries: 5
                   restart: unless-stopped
               """;
    }

    private static void PrintInstallSummary(
        ICliConsole console,
        string releaseTag,
        string imageTag,
        InstallOptions options)
    {
        var panelUrl = $"http://0.0.0.0:{options.ApiPort}{options.ApiPrefix}/";
        var apiUrl = $"http://0.0.0.0:{options.ApiPort}{options.ApiPrefix}/api";

        console.Header("XRayne API installation completed");
        console.Value("Release", releaseTag);
        console.Value("Docker image", $"{CliDefaults.ImageName}:{imageTag}");
        console.Value("Project path", options.Paths.Root);
        console.Value("Environment file", options.Paths.EnvConfig);
        console.Value("Config file", options.Paths.JsonConfig);
        console.Value("Compose file", options.Paths.DockerCompose);

        console.Section("Panel");
        console.Value("URL", panelUrl);
        console.Value("API URL", apiUrl);
        console.Value("Prefix", string.IsNullOrWhiteSpace(options.ApiPrefix) ? "(none)" : options.ApiPrefix);

        console.Section("PostgreSQL");
        console.Value("API host", "postgres:5432");
        console.Value("CLI host", "localhost:5432");
        console.Value("Database", CliDefaults.PostgresDatabase);
        console.Value("Username", CliDefaults.PostgresUser);
        console.Value("Password", options.PostgresPassword);

        console.Section("Project folders");
        console.Value("Logs", options.Paths.LogsDirectory);
        console.Value("Xray", options.Paths.XrayDirectory);
        console.Value("PostgreSQL data", options.Paths.PostgresDirectory);
        console.Value("Container project", "/app/shared");

        console.Section("Next useful commands");
        console.Command($"cd {options.Paths.Root}");
        console.Command("docker compose ps");
        console.Command("docker compose logs -f api");
        console.Command("docker compose logs -f postgres");
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

    private static string EscapeEnvironmentValue(string value)
    {
        if (value.Length == 0 || !value.Any(character => char.IsWhiteSpace(character) || character is '#' or '=' or '"' or '\''))
        {
            return value;
        }

        return $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
    }

    private sealed class InstallOptions
    {
        public int ApiPort { get; set; }
        public string PostgresPassword { get; set; } = string.Empty;
        public string ApiPrefix { get; set; } = string.Empty;
        public ProjectPaths Paths { get; }

        public InstallOptions(string projectPath)
        {
            Paths = new ProjectPaths(projectPath);
        }
    }
}
