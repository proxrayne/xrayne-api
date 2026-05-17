using XRayne.Contracts.Configurations;
using XRayne.Contracts.Values;
using XRayne.Infrastructure.Services.PanelSettings;

namespace XRayne.Test.Services.PanelSettings;

public sealed class ProjectPathResolverTests
{
    [Fact]
    public void CertificatesDirectory_WhenAccessorOverrides_ReturnsOverride()
    {
        var accessor = Substitute.For<IPanelSettingsAccessor>();
        accessor.Current.Returns(new PanelOptions { CertificatesDirectory = "/custom/certs" });

        var resolver = new ProjectPathResolver(accessor);

        resolver.CertificatesDirectory.Should().Be("/custom/certs");
    }

    [Fact]
    public void CertificatesDirectory_WhenAccessorNull_ReturnsPathProviderDefault()
    {
        var accessor = Substitute.For<IPanelSettingsAccessor>();
        accessor.Current.Returns(new PanelOptions { CertificatesDirectory = null });

        var resolver = new ProjectPathResolver(accessor);

        resolver.CertificatesDirectory.Should().Be(PathProvider.Paths.CertificatesDirectory);
    }

    [Fact]
    public void GeoResourcesDirectory_WhenAccessorOverrides_ReturnsOverride()
    {
        var accessor = Substitute.For<IPanelSettingsAccessor>();
        accessor.Current.Returns(new PanelOptions { GeoResourcesDirectory = "/custom/geo" });

        var resolver = new ProjectPathResolver(accessor);

        resolver.GeoResourcesDirectory.Should().Be("/custom/geo");
    }

    [Fact]
    public void GeoResourcesDirectory_WhenAccessorNull_ReturnsPathProviderDefault()
    {
        var accessor = Substitute.For<IPanelSettingsAccessor>();
        accessor.Current.Returns(new PanelOptions { GeoResourcesDirectory = null });

        var resolver = new ProjectPathResolver(accessor);

        resolver.GeoResourcesDirectory.Should().Be(PathProvider.Paths.GeoResourcesDirectory);
    }

    [Fact]
    public void Root_AlwaysProxiesPathProvider_IgnoringAccessor()
    {
        var accessor = Substitute.For<IPanelSettingsAccessor>();
        accessor.Current.Returns(new PanelOptions());

        var resolver = new ProjectPathResolver(accessor);

        resolver.Root.Should().Be(PathProvider.Paths.Root);
        resolver.LogsDirectory.Should().Be(PathProvider.Paths.LogsDirectory);
        resolver.PostgresDirectory.Should().Be(PathProvider.Paths.PostgresDirectory);
        resolver.JsonConfig.Should().Be(PathProvider.Paths.JsonConfig);
        resolver.EnvConfig.Should().Be(PathProvider.Paths.EnvConfig);
        resolver.DockerCompose.Should().Be(PathProvider.Paths.DockerCompose);
    }

    [Fact]
    public void CertificatesDirectory_RespondsImmediately_ToAccessorChange()
    {
        var accessor = Substitute.For<IPanelSettingsAccessor>();
        var current = new PanelOptions { CertificatesDirectory = "/initial" };
        accessor.Current.Returns(_ => current);

        var resolver = new ProjectPathResolver(accessor);

        resolver.CertificatesDirectory.Should().Be("/initial");

        current = new PanelOptions { CertificatesDirectory = "/changed" };
        resolver.CertificatesDirectory.Should().Be("/changed");
    }

    [Fact]
    public void OverridableDirectory_WhenAccessorReturnsEmpty_TreatsAsAbsentAndFallsBack()
    {
        var accessor = Substitute.For<IPanelSettingsAccessor>();
        accessor.Current.Returns(new PanelOptions
        {
            CertificatesDirectory = "   ",
            GeoResourcesDirectory = ""
        });

        var resolver = new ProjectPathResolver(accessor);

        resolver.CertificatesDirectory.Should().Be(PathProvider.Paths.CertificatesDirectory);
        resolver.GeoResourcesDirectory.Should().Be(PathProvider.Paths.GeoResourcesDirectory);
    }
}
