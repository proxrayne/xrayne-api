using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using XRayne.Repositories.Abstractions;
using XRayne.Repositories.Configuration;
using XRayne.Repositories.Persistence;
using XRayne.Repositories.PostgreSql;

namespace XRayne.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddXRayneRepositories(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PostgreSqlOptions>(configuration.GetSection("PostgreSql"));

        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? configuration.GetSection("PostgreSql").Get<PostgreSqlOptions>()?.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("PostgreSQL connection string is not configured.");
        }

        services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));

        services.AddDbContext<XRayneDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IPostgreSqlConnectionFactory, PostgreSqlConnectionFactory>();

        return services;
    }
}
