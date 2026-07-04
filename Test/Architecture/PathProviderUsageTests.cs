using Contracts.Values;

namespace Test.Architecture;

/// <summary>
/// Verifies runtime path derivation used by backend configuration.
/// </summary>
public sealed class PathProviderUsageTests
{
    /// <summary>
    /// Ensures all runtime paths remain rooted under the configured project directory.
    /// </summary>
    [Fact]
    public void ProjectPaths_DerivesRuntimePathsFromRoot()
    {
        var paths = new ProjectPaths("/opt/xrayne");

        paths.Root.Should().Be("/opt/xrayne");
        paths.XrayDirectory.Should().Be(Path.Combine("/opt/xrayne", "xray"));
        paths.LogsDirectory.Should().Be(Path.Combine("/opt/xrayne", "logs"));
        paths.PostgresDirectory.Should().Be(Path.Combine("/opt/xrayne", "postgres"));
        paths.DownloadsDirectory.Should().Be(Path.Combine("/opt/xrayne", "downloads"));
        paths.CertificatesDirectory.Should().Be(Path.Combine("/opt/xrayne", "certificates"));
        paths.LetsEncryptDirectory.Should().Be(Path.Combine("/opt/xrayne", "certificates", "letsencrypt"));
        paths.GeoResourcesDirectory.Should().Be(Path.Combine("/opt/xrayne", "xray", "geo"));
        paths.JsonConfig.Should().Be(Path.Combine("/opt/xrayne", "config.json"));
        paths.EnvConfig.Should().Be(Path.Combine("/opt/xrayne", ".env"));
        paths.DockerCompose.Should().Be(Path.Combine("/opt/xrayne", "docker-compose.yml"));
    }
}
