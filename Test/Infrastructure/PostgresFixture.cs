using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Data;

namespace Test.Infrastructure;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("xrayne_tests")
        .WithUsername("xrayne")
        .WithPassword("xrayne")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = await AppDbContextFactory.CreateAsync(ConnectionString);
    }

    public async Task ResetAsync()
    {
        await using var context = new DbContextOptionsBuilder<AppDbContext>()
            .UseXRayneNpgsql(ConnectionString)
            .Options
            .CreateContext();

        await context.Database.ExecuteSqlRawAsync(
            """
            TRUNCATE TABLE "AdminAccounts" RESTART IDENTITY CASCADE;
            """);
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

internal static class DbContextOptionsExtensions
{
    public static AppDbContext CreateContext(this DbContextOptions<AppDbContext> options) => new(options);
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres";
}
