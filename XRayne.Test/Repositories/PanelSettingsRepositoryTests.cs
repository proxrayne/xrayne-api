using Microsoft.EntityFrameworkCore;
using XRayne.Repositories.Panel;
using XRayne.Test.Infrastructure;

namespace XRayne.Test.Repositories;

[Collection(PostgresCollection.Name)]
public sealed class PanelSettingsRepositoryTests
{
    private readonly PostgresFixture _postgres;

    public PanelSettingsRepositoryTests(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task GetAsync_WhenTableEmpty_CreatesAndReturnsDefaults()
    {
        await _postgres.ResetAsync();
        await using var context = await AppDbContextFactory.CreateAsync(_postgres.ConnectionString);
        var repo = new PanelSettingsRepository(context);

        var settings = await repo.GetAsync();

        settings.Should().NotBeNull();
        settings.Port.Should().Be(5097);
        settings.WebBasePath.Should().Be("/");
        settings.SessionLifetimeMinutes.Should().Be(7200);
        settings.PendingRestart.Should().BeFalse();
        settings.BindIp.Should().BeNull();
        settings.Domain.Should().BeNull();

        (await context.PanelSettings.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task GetAsync_WhenCalledTwice_ReturnsSameRow()
    {
        await _postgres.ResetAsync();
        await using var context = await AppDbContextFactory.CreateAsync(_postgres.ConnectionString);
        var repo = new PanelSettingsRepository(context);

        var first = await repo.GetAsync();
        var second = await repo.GetAsync();

        first.Id.Should().Be(second.Id);
        (await context.PanelSettings.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges_AndUpdatesTimestamp()
    {
        await _postgres.ResetAsync();
        await using var context = await AppDbContextFactory.CreateAsync(_postgres.ConnectionString);
        var repo = new PanelSettingsRepository(context);

        var settings = await repo.GetAsync();
        var originalUpdatedAt = settings.UpdatedAt;

        await Task.Delay(20);

        settings.Port = 8443;
        settings.Domain = "https://panel.example";
        await repo.UpdateAsync(settings);

        await using var verify = await AppDbContextFactory.CreateAsync(_postgres.ConnectionString);
        var reloaded = await verify.PanelSettings.SingleAsync();
        reloaded.Port.Should().Be(8443);
        reloaded.Domain.Should().Be("https://panel.example");
        reloaded.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task SetPendingRestartAsync_TogglesFlag_Independently()
    {
        await _postgres.ResetAsync();
        await using var context = await AppDbContextFactory.CreateAsync(_postgres.ConnectionString);
        var repo = new PanelSettingsRepository(context);

        var settings = await repo.GetAsync();
        settings.Port = 7777;
        await repo.UpdateAsync(settings);

        await repo.SetPendingRestartAsync(true);

        await using var verify = await AppDbContextFactory.CreateAsync(_postgres.ConnectionString);
        var reloaded = await verify.PanelSettings.SingleAsync();
        reloaded.PendingRestart.Should().BeTrue();
        reloaded.Port.Should().Be(7777);

        await repo.SetPendingRestartAsync(false);

        await using var verify2 = await AppDbContextFactory.CreateAsync(_postgres.ConnectionString);
        var reloaded2 = await verify2.PanelSettings.SingleAsync();
        reloaded2.PendingRestart.Should().BeFalse();
        reloaded2.Port.Should().Be(7777);
    }
}
