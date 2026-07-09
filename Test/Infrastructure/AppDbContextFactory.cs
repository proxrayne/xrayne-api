using Microsoft.EntityFrameworkCore;
using Data;

namespace Test.Infrastructure;

internal static class AppDbContextFactory
{
    public static async Task<AppDbContext> CreateAsync(string connectionString)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseXRayneNpgsql(connectionString)
            .Options;

        var context = new AppDbContext(options);
        await context.Database.MigrateAsync();

        return context;
    }
}
