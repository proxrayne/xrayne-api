using Microsoft.EntityFrameworkCore;
using XRayne.Repositories;

namespace XRayne.Test.Infrastructure;

internal static class AppDbContextFactory
{
    public static async Task<AppDbContext> CreateAsync(string connectionString)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        var context = new AppDbContext(options);
        await context.Database.MigrateAsync();

        return context;
    }
}
