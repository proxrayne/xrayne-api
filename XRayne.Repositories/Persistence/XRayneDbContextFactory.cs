using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using XRayne.Repositories.Configuration;

namespace XRayne.Repositories.Persistence;

public sealed class XRayneDbContextFactory : IDesignTimeDbContextFactory<XRayneDbContext>
{
    public XRayneDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Development";

        var basePath = Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables("XRAYNE_")
            .Build();

        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? configuration.GetSection("PostgreSql").Get<PostgreSqlOptions>()?.ConnectionString;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("PostgreSQL connection string is not configured.");
        }

        var options = new DbContextOptionsBuilder<XRayneDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new XRayneDbContext(options);
    }
}
