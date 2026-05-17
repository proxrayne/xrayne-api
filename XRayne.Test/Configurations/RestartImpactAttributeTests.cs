using System.Reflection;
using XRayne.Contracts.Configurations;

namespace XRayne.Test.Configurations;

public sealed class RestartImpactAttributeTests
{
    [Fact]
    public void EveryPropertyOf_PanelOptions_HasRestartImpactAttribute()
    {
        var properties = typeof(PanelSettings)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute<RestartImpactAttribute>();
            attribute.Should().NotBeNull(
                "property {0} on PanelOptions must declare a [RestartImpact(...)] so the diff classifier knows whether a change requires restart",
                property.Name);
        }
    }

    [Theory]
    [InlineData(nameof(PanelSettings.BindIp), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelSettings.Port), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelSettings.CertPublicKeyPath), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelSettings.CertPrivateKeyPath), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelSettings.Domain), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelSettings.PathBase), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelSettings.SessionLifetimeMinutes), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelSettings.TrustedProxyCidrs), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelSettings.CertificatesDirectory), RestartImpact.HotReload)]
    [InlineData(nameof(PanelSettings.GeoResourcesDirectory), RestartImpact.HotReload)]
    public void Property_HasExpectedImpact(string propertyName, RestartImpact expected)
    {
        var property = typeof(PanelSettings).GetProperty(propertyName)!;
        var actual = property.GetCustomAttribute<RestartImpactAttribute>()!.Impact;
        actual.Should().Be(expected);
    }
}
