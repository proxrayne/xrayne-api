using Microsoft.EntityFrameworkCore;
using XRayne.Contracts.Enums;
using XRayne.Repositories.Entities;

namespace XRayne.Repositories;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AdminAccount> AdminAccounts => Set<AdminAccount>();
    public DbSet<InboundEntity> Inbounds => Set<InboundEntity>();
    public DbSet<OutboundEntity> Outbounds => Set<OutboundEntity>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<UserStatus>();
        modelBuilder.HasPostgresEnum<LimitResetStrategy>();
        modelBuilder.HasPostgresEnum<AdminPermission>();

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var property = entity.FindProperty(nameof(CreateUpdateEntity.CreatedAt));
            if (property != null)
            {
                property.SetDefaultValueSql("CURRENT_TIMESTAMP");
            }
        }

        modelBuilder.Entity<InboundEntity>(builder =>
        {
            builder.Property(x => x.Enabled)
                .HasColumnName("Enabled")
                .HasDefaultValue(true);
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Enum>()
            .HaveConversion<string>();
    }
}
