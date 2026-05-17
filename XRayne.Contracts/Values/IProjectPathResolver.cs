namespace XRayne.Contracts.Values;

public interface IProjectPathResolver
{
    string Root { get; }
    string XrayDirectory { get; }
    string LogsDirectory { get; }
    string PostgresDirectory { get; }
    string DownloadsDirectory { get; }
    string CertificatesDirectory { get; }
    string LetsEncryptDirectory { get; }
    string GeoResourcesDirectory { get; }
    string JsonConfig { get; }
    string EnvConfig { get; }
    string DockerCompose { get; }
}
