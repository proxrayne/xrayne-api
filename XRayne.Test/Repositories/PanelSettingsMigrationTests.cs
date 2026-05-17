using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using XRayne.Repositories;

namespace XRayne.Test.Repositories;

public sealed class PanelSettingsMigrationTests
{
    [Fact]
    public async Task Migration_AppliesCleanly_OnFreshDb()
    {
        await using var container = BuildContainer();
        await container.StartAsync();

        await using var context = CreateContext(container.GetConnectionString());
        await context.Database.MigrateAsync();

        await using var conn = new NpgsqlConnection(container.GetConnectionString());
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT column_name FROM information_schema.columns WHERE table_name = 'panel_settings' ORDER BY ordinal_position",
            conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var columns = new List<string>();
        while (await reader.ReadAsync())
        {
            columns.Add(reader.GetString(0));
        }

        columns.Should().Contain([
            "Id", "BindIp", "Domain", "Port", "WebBasePath", "SessionLifetimeMinutes",
            "TrustedProxyCidrs", "CertificatesDirectory", "GeoResourcesDirectory",
            "PanelCertPublicKeyPath", "PanelCertPrivateKeyPath", "PendingRestart", "UpdatedAt"
        ]);
    }

    [Fact]
    public async Task Migration_IsIdempotent_OnExistingDb()
    {
        await using var container = BuildContainer();
        await container.StartAsync();

        await using (var context = CreateContext(container.GetConnectionString()))
        {
            await context.Database.MigrateAsync();
        }

        await using var second = CreateContext(container.GetConnectionString());
        var act = async () => await second.Database.MigrateAsync();

        await act.Should().NotThrowAsync();
    }

    private static PostgreSqlContainer BuildContainer() =>
        new PostgreSqlBuilder("postgres:17-alpine")
            .WithDatabase("xrayne_migration_tests")
            .WithUsername("xrayne")
            .WithPassword("xrayne")
            .Build();

    private static AppDbContext CreateContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
