using Microsoft.EntityFrameworkCore;

namespace XRayne.Repositories.Persistence;

public sealed class XRayneDbContext : DbContext
{
    public XRayneDbContext(DbContextOptions<XRayneDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("xrayne");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(XRayneDbContext).Assembly);
    }
}
