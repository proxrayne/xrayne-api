using XRayne.Contracts.Configurations;
using XRayne.Infrastructure.Services.PanelSettings;

namespace XRayne.Test.Services.PanelSettings;

public sealed class SettingsDiffTests
{
    [Fact]
    public void Compute_WhenNothingChanged_ReturnsEmptyAndNone()
    {
        var options = new PanelOptions();

        var result = SettingsDiff.Compute(options, options.Clone());

        result.ChangedFields.Should().BeEmpty();
        result.MaxImpact.Should().Be(RestartImpact.None);
    }

    [Fact]
    public void Compute_WhenOnlyHotReloadField_ReturnsHotReload()
    {
        var left = new PanelOptions();
        var right = left.Clone();
        right.CertificatesDirectory = "/custom/certs";

        var result = SettingsDiff.Compute(left, right);

        result.ChangedFields.Should().ContainSingle().Which.Should().Be("certificatesDirectory");
        result.MaxImpact.Should().Be(RestartImpact.HotReload);
    }

    [Fact]
    public void Compute_WhenOnlyFullRestartField_ReturnsFullRestart()
    {
        var left = new PanelOptions();
        var right = left.Clone();
        right.Port = 5098;

        var result = SettingsDiff.Compute(left, right);

        result.ChangedFields.Should().ContainSingle().Which.Should().Be("port");
        result.MaxImpact.Should().Be(RestartImpact.FullRestart);
    }

    [Fact]
    public void Compute_WhenMixed_ReturnsFullRestartAsMax()
    {
        var left = new PanelOptions();
        var right = left.Clone();
        right.CertificatesDirectory = "/x";
        right.Port = 5098;

        var result = SettingsDiff.Compute(left, right);

        result.ChangedFields.Should().HaveCount(2);
        result.MaxImpact.Should().Be(RestartImpact.FullRestart);
    }

    [Theory]
    [InlineData(null, "value")]
    [InlineData("value", null)]
    public void Compute_DetectsNullVsValue(string? leftValue, string? rightValue)
    {
        var left = new PanelOptions { Domain = leftValue };
        var right = new PanelOptions { Domain = rightValue };

        var result = SettingsDiff.Compute(left, right);

        result.ChangedFields.Should().Contain("domain");
    }

    [Fact]
    public void Compute_TreatsWhitespaceStrings_AsDistinct()
    {
        var left = new PanelOptions { Domain = "" };
        var right = new PanelOptions { Domain = " " };

        var result = SettingsDiff.Compute(left, right);

        result.ChangedFields.Should().Contain("domain");
    }

    [Fact]
    public void Compute_IsCaseSensitiveForStrings()
    {
        var left = new PanelOptions { Domain = "abc" };
        var right = new PanelOptions { Domain = "ABC" };

        var result = SettingsDiff.Compute(left, right);

        result.ChangedFields.Should().Contain("domain");
    }

    [Fact]
    public void Compute_DetectsAllPropertyChanges()
    {
        var left = new PanelOptions();
        var right = new PanelOptions
        {
            BindIp = "127.0.0.1",
            Domain = "https://example.com",
            Port = 9999,
            WebBasePath = "/x/",
            SessionLifetimeMinutes = 60,
            TrustedProxyCidrs = "10.0.0.0/8",
            CertificatesDirectory = "/certs",
            GeoResourcesDirectory = "/geo",
            PanelCertPublicKeyPath = "/c/pub",
            PanelCertPrivateKeyPath = "/c/priv"
        };

        var result = SettingsDiff.Compute(left, right);

        result.ChangedFields.Should().HaveCount(10);
        result.MaxImpact.Should().Be(RestartImpact.FullRestart);
    }

    [Fact]
    public void Compute_NullArgument_Throws()
    {
        ((Action)(() => SettingsDiff.Compute(null!, new PanelOptions())))
            .Should().Throw<ArgumentNullException>();
    }
}
