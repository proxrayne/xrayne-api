using Microsoft.EntityFrameworkCore;
using XRayne.Contracts.Enums;
using XRayne.Repositories.Entities;

namespace XRayne.Repositories;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AdminAccount> AdminAccounts { get; set; }
    public DbSet<InboundEntity> Inbounds { get; set; }
    public DbSet<OutboundEntity> Outbounds { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<NodeEntity> Nodes { get; set; }
    public DbSet<CertificateEntity> Certificates { get; set; }
    public DbSet<GeoResourceEntity> GeoResources { get; set; }
    public DbSet<RoutingRuleEntity> RoutingRules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<UserStatus>();
        modelBuilder.HasPostgresEnum<LimitResetStrategy>();
        modelBuilder.HasPostgresEnum<AdminPermission>();
        modelBuilder.HasPostgresEnum<SSHAuthType>();
        modelBuilder.HasPostgresEnum<NodeStatus>();

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

        modelBuilder.Entity<OutboundEntity>(builder =>
        {
            builder.Property(x => x.Enabled)
                .HasColumnName("Enabled")
                .HasDefaultValue(true);
        });

        modelBuilder.Entity<NodeEntity>(builder =>
        {
            builder.HasMany(x => x.Inbounds)
                .WithOne(x => x.Node)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Outbounds)
                .WithOne(x => x.Node)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.RoutingRules)
                .WithOne(x => x.Node)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CertificateEntity>(builder =>
        {
            builder.HasOne(x => x.Node)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GeoResourceEntity>(builder =>
        {
            builder.HasOne(x => x.Node)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Enum>()
            .HaveConversion<string>();
    }
}
