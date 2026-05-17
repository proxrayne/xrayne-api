using XRayne.Contracts.Values;

namespace XRayne.Infrastructure.Services.PanelSettings;

public sealed class ProjectPathResolver(IPanelSettingsAccessor accessor) : IProjectPathResolver
{
    public string Root => PathProvider.Paths.Root;

    public string XrayDirectory => PathProvider.Paths.XrayDirectory;

    public string LogsDirectory => PathProvider.Paths.LogsDirectory;

    public string PostgresDirectory => PathProvider.Paths.PostgresDirectory;

    public string DownloadsDirectory => PathProvider.Paths.DownloadsDirectory;

    public string CertificatesDirectory =>
        WhenNotEmpty(accessor.Current.CertificatesDirectory) ?? PathProvider.Paths.CertificatesDirectory;

    public string LetsEncryptDirectory => PathProvider.Paths.LetsEncryptDirectory;

    public string GeoResourcesDirectory =>
        WhenNotEmpty(accessor.Current.GeoResourcesDirectory) ?? PathProvider.Paths.GeoResourcesDirectory;

    public string JsonConfig => PathProvider.Paths.JsonConfig;

    public string EnvConfig => PathProvider.Paths.EnvConfig;

    public string DockerCompose => PathProvider.Paths.DockerCompose;

    private static string? WhenNotEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
