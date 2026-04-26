using Microsoft.EntityFrameworkCore;
using XRayne.Repositories.Entities;

namespace XRayne.Repositories;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AdminAccount> AdminAccounts => Set<AdminAccount>();
}
