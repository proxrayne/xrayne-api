using Octokit;

namespace Infrastructure.Services;

public sealed class GitHubReleaseClient : IDisposable
{
    private const string LatestVersion = "latest";

    private readonly GitHubClient gitHubClient;
    private readonly HttpClient httpClient;
    private readonly string owner;
    private readonly string repositoryName;

    public GitHubReleaseClient(string repository)
        : this(repository, new HttpClient())
    {
    }

    public GitHubReleaseClient(string repository, HttpClient httpClient)
    {
        FullName = NormalizeRepositoryFullName(repository);
        var parts = FullName.Split('/', 2);

        owner = parts[0];
        repositoryName = parts[1];
        this.httpClient = httpClient;
        gitHubClient = new GitHubClient(new ProductHeaderValue("xrayne-panel"));
    }

    public string FullName { get; }

    public static string NormalizeRepositoryFullName(string repository)
    {
        if (string.IsNullOrWhiteSpace(repository))
        {
            throw new ArgumentException("Repository is required.", nameof(repository));
        }

        var value = repository.Trim().TrimEnd('/');
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            if (!string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only github.com repository URLs are supported.", nameof(repository));
            }

            value = uri.AbsolutePath.Trim('/');
        }

        var parts = value.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            throw new ArgumentException(
                "Repository must be a GitHub URL or an 'owner/name' value.",
                nameof(repository));
        }

        return $"{parts[0]}/{parts[1]}";
    }

    public async Task<IReadOnlyList<Release>> GetReleasesAsync(
        int? perPage,
        int? page,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var options = new ApiOptions
        {
            PageCount = 1
        };

        if (perPage is > 0)
        {
            options.PageSize = perPage.Value;
        }

        if (page is > 0)
        {
            options.StartPage = page.Value;
        }

        var releases = await gitHubClient.Repository.Release.GetAll(owner, repositoryName, options);
        cancellationToken.ThrowIfCancellationRequested();

        return releases;
    }

    public async Task<Release> GetReleaseAsync(string version, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var release = string.Equals(version, LatestVersion, StringComparison.OrdinalIgnoreCase)
            ? await gitHubClient.Repository.Release.GetLatest(owner, repositoryName)
            : await gitHubClient.Repository.Release.Get(owner, repositoryName, version);

        cancellationToken.ThrowIfCancellationRequested();

        return release;
    }

    public async Task<string> DownloadAssetAsync(
        ReleaseAsset asset,
        string destinationDirectory,
        string assetName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(asset.BrowserDownloadUrl))
        {
            throw new InvalidOperationException($"Release asset '{asset.Name}' does not have a download URL.");
        }

        Directory.CreateDirectory(destinationDirectory);

        var destinationPath = Path.Combine(destinationDirectory, assetName);
        await using var output = File.Create(destinationPath);
        using var response = await httpClient.GetAsync(
            asset.BrowserDownloadUrl,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        await response.Content.CopyToAsync(output, cancellationToken);

        return destinationPath;
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
}
