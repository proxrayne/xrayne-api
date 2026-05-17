using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using XRayne.Contracts.Configurations;
using XRayne.Infrastructure.Services.PanelSettings;
using XRayne.Repositories;
using XRayne.Repositories.Panel;
using XRayne.Test.Infrastructure;

namespace XRayne.Test.Services.PanelSettings;

[Collection(PostgresCollection.Name)]
public sealed class PanelSettingsAccessorTests
{
    private readonly PostgresFixture _postgres;

    public PanelSettingsAccessorTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    private async Task<(IServiceScopeFactory ScopeFactory, ServiceProvider Provider)> BuildScopeAsync()
    {
        await _postgres.ResetAsync();

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(_postgres.ConnectionString));
        services.AddScoped<IPanelSettingsRepository, PanelSettingsRepository>();
        var provider = services.BuildServiceProvider();
        return (provider.GetRequiredService<IServiceScopeFactory>(), provider);
    }

    [Fact]
    public async Task RefreshFromStoreAsync_OnFreshDb_LoadsDefaults()
    {
        var (scopes, provider) = await BuildScopeAsync();
        await using var _ = provider;

        var accessor = new PanelSettingsAccessor(scopes, NullLogger<PanelSettingsAccessor>.Instance);
        await accessor.RefreshFromStoreAsync();

        accessor.Current.Port.Should().Be(5097);
        accessor.Current.WebBasePath.Should().Be("/");
        accessor.PendingRestart.Should().BeFalse();
    }

    [Fact]
    public async Task ApplyAsync_PersistsToDb_AndUpdatesCurrent()
    {
        var (scopes, provider) = await BuildScopeAsync();
        await using var _ = provider;

        var accessor = new PanelSettingsAccessor(scopes, NullLogger<PanelSettingsAccessor>.Instance);
        await accessor.RefreshFromStoreAsync();

        var next = accessor.Current.Clone();
        next.CertificatesDirectory = "/x";
        next.GeoResourcesDirectory = "/y";

        var result = await accessor.ApplyAsync(next);

        accessor.Current.CertificatesDirectory.Should().Be("/x");
        accessor.Current.GeoResourcesDirectory.Should().Be("/y");
        result.ChangedFields.Should().BeEquivalentTo(["certificatesDirectory", "geoResourcesDirectory"]);
        result.MaxImpact.Should().Be(RestartImpact.HotReload);
    }

    [Fact]
    public async Task ApplyAsync_WithFullRestartChange_SetsPendingRestart()
    {
        var (scopes, provider) = await BuildScopeAsync();
        await using var _ = provider;

        var accessor = new PanelSettingsAccessor(scopes, NullLogger<PanelSettingsAccessor>.Instance);
        await accessor.RefreshFromStoreAsync();

        var next = accessor.Current.Clone();
        next.Port = 5098;

        var result = await accessor.ApplyAsync(next);

        accessor.PendingRestart.Should().BeTrue();
        result.RequiresRestart.Should().BeTrue();
        result.HotReloadedFields.Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyAsync_WithHotReloadOnly_DoesNotSetPendingRestart()
    {
        var (scopes, provider) = await BuildScopeAsync();
        await using var _ = provider;

        var accessor = new PanelSettingsAccessor(scopes, NullLogger<PanelSettingsAccessor>.Instance);
        await accessor.RefreshFromStoreAsync();

        var next = accessor.Current.Clone();
        next.CertificatesDirectory = "/x";

        await accessor.ApplyAsync(next);

        accessor.PendingRestart.Should().BeFalse();
    }

    [Fact]
    public async Task Subscribe_InvokesHandler_OnApply()
    {
        var (scopes, provider) = await BuildScopeAsync();
        await using var _ = provider;

        var accessor = new PanelSettingsAccessor(scopes, NullLogger<PanelSettingsAccessor>.Instance);
        await accessor.RefreshFromStoreAsync();

        PanelOptions? received = null;
        using var subscription = accessor.Subscribe(o => received = o);

        var next = accessor.Current.Clone();
        next.CertificatesDirectory = "/x";
        await accessor.ApplyAsync(next);

        received.Should().NotBeNull();
        received!.CertificatesDirectory.Should().Be("/x");
    }

    [Fact]
    public async Task Subscribe_DoesNotInvokeHandler_AfterDispose()
    {
        var (scopes, provider) = await BuildScopeAsync();
        await using var _ = provider;

        var accessor = new PanelSettingsAccessor(scopes, NullLogger<PanelSettingsAccessor>.Instance);
        await accessor.RefreshFromStoreAsync();

        var calls = 0;
        var subscription = accessor.Subscribe(_ => calls++);
        subscription.Dispose();

        var next = accessor.Current.Clone();
        next.CertificatesDirectory = "/x";
        await accessor.ApplyAsync(next);

        calls.Should().Be(0);
    }

    [Fact]
    public async Task ClearPendingRestartAsync_ResetsFlag_WithoutTouchingOtherFields()
    {
        var (scopes, provider) = await BuildScopeAsync();
        await using var _ = provider;

        var accessor = new PanelSettingsAccessor(scopes, NullLogger<PanelSettingsAccessor>.Instance);
        await accessor.RefreshFromStoreAsync();

        var next = accessor.Current.Clone();
        next.Port = 9999;
        await accessor.ApplyAsync(next);

        accessor.PendingRestart.Should().BeTrue();
        await accessor.ClearPendingRestartAsync();
        accessor.PendingRestart.Should().BeFalse();
        accessor.Current.Port.Should().Be(9999);
    }

    [Fact]
    public async Task ApplyAsync_WhenNoChanges_ReturnsEmptyDiff_AndDoesNotNotify()
    {
        var (scopes, provider) = await BuildScopeAsync();
        await using var _ = provider;

        var accessor = new PanelSettingsAccessor(scopes, NullLogger<PanelSettingsAccessor>.Instance);
        await accessor.RefreshFromStoreAsync();

        var calls = 0;
        using var _sub = accessor.Subscribe(_ => calls++);

        var result = await accessor.ApplyAsync(accessor.Current.Clone());

        result.ChangedFields.Should().BeEmpty();
        calls.Should().Be(0);
    }
}
