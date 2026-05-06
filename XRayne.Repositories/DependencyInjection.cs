using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using XRayne.Repositories.Admins;

namespace XRayne.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = GetEnvConnectionString(configuration) ?? configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("PostgreSQL connection string is not configured.");
        }

        services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IAdminAccountRepository, AdminAccountRepository>();

        return services;
    }

    private static string? GetEnvConnectionString(IConfiguration configuration)
    {
        var user = configuration["POSTGRES_USER"];
        var password = configuration["POSTGRES_PASSWORD"];
        var database = configuration["POSTGRES_DB"];
        if (string.IsNullOrWhiteSpace(user)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(database))
        {
            return null;
        }

        var host = configuration["POSTGRES_HOST"];
        if (string.IsNullOrWhiteSpace(host))
        {
            host = configuration["POSTGRES_HOST_API"];
        }

        if (string.IsNullOrWhiteSpace(host))
        {
            host = "localhost";
        }

        var port = configuration["POSTGRES_PORT"];
        if (string.IsNullOrWhiteSpace(port))
        {
            port = configuration["POSTGRES_CONTAINER_PORT"];
        }

        if (string.IsNullOrWhiteSpace(port))
        {
            port = "5432";
        }

        return $"Host={host};Port={port};Username={user};Password={password};Database={database}";
    }
}
