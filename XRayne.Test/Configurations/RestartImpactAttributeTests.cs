using System.Reflection;
using XRayne.Contracts.Configurations;

namespace XRayne.Test.Configurations;

public sealed class RestartImpactAttributeTests
{
    [Fact]
    public void EveryPropertyOf_PanelOptions_HasRestartImpactAttribute()
    {
        var properties = typeof(PanelOptions)
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
    [InlineData(nameof(PanelOptions.BindIp), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelOptions.Port), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelOptions.PanelCertPublicKeyPath), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelOptions.PanelCertPrivateKeyPath), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelOptions.Domain), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelOptions.WebBasePath), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelOptions.SessionLifetimeMinutes), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelOptions.TrustedProxyCidrs), RestartImpact.FullRestart)]
    [InlineData(nameof(PanelOptions.CertificatesDirectory), RestartImpact.HotReload)]
    [InlineData(nameof(PanelOptions.GeoResourcesDirectory), RestartImpact.HotReload)]
    public void Property_HasExpectedImpact(string propertyName, RestartImpact expected)
    {
        var property = typeof(PanelOptions).GetProperty(propertyName)!;
        var actual = property.GetCustomAttribute<RestartImpactAttribute>()!.Impact;
        actual.Should().Be(expected);
    }
}
