using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using XRayne.Contracts.Enums;
using XRayne.Infrastructure.Services;
using XRayne.Infrastructure.Utilities;
using XRayne.Repositories;
using XRayne.Repositories.Admins;
using XRayne.Repositories.Entities;

namespace XRayne.Test.Infrastructure;

public sealed class XRayneWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly string? _previousConnectionString;

    public XRayneWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
        _previousConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", connectionString);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            ReplaceDbContext(services);
            services.RemoveAll<NpgsqlDataSource>();
            services.AddSingleton(_ => NpgsqlDataSource.Create(_connectionString));
            services.RemoveHostedServices();
        });
    }

    private void ReplaceDbContext(IServiceCollection services)
    {
        services.RemoveAll<DbContextOptions<AppDbContext>>();
        services.RemoveAll<DbContextOptions>();
        services.RemoveAll<AppDbContext>();
        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(_connectionString));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__Default", _previousConnectionString);
        }
    }

    public async Task<AdminAccount> CreateAdminAsync(
        string username,
        AdminPermission permissions = AdminPermission.SuperAdmin,
        CancellationToken ct = default)
    {
        using var scope = Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAdminAccountRepository>();

        var account = new AdminAccount
        {
            Username = username,
            PasswordHash = IdentityPasswordHasher.HashPassword("test-password"),
            Permissions = permissions
        };

        await repo.AddAsync(account, ct);

        return account;
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(
        AdminPermission permissions = AdminPermission.SuperAdmin,
        CancellationToken ct = default)
    {
        var admin = await CreateAdminAsync($"admin-{Guid.NewGuid():N}", permissions, ct);

        using var scope = Services.CreateScope();
        var tokens = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var token = tokens.CreateAccessToken(admin.Id, admin.Username, admin.Permissions);

        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }
}

internal static class ServiceCollectionExtensions
{
    public static void RemoveAll<TService>(this IServiceCollection services)
    {
        for (var i = services.Count - 1; i >= 0; i--)
        {
            if (services[i].ServiceType == typeof(TService))
            {
                services.RemoveAt(i);
            }
        }
    }

    public static void RemoveHostedServices(this IServiceCollection services)
    {
        // Снимаем только Quartz — его scheduler не нужен в HTTP-тестах.
        // App-level hosted services (например, PanelSettingsBootstrapService) оставляем,
        // чтобы тесты проходили через реальный startup.
        for (var i = services.Count - 1; i >= 0; i--)
        {
            if (services[i].ServiceType != typeof(IHostedService))
            {
                continue;
            }

            var implTypeName = services[i].ImplementationType?.FullName
                ?? services[i].ImplementationInstance?.GetType().FullName
                ?? string.Empty;

            if (implTypeName.StartsWith("Quartz", StringComparison.Ordinal))
            {
                services.RemoveAt(i);
            }
        }
    }
}
