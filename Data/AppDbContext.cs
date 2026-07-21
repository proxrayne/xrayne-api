using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AdminAccountEntity> AdminAccounts { get; set; }
    public DbSet<InboundEntity> Inbounds { get; set; }
    public DbSet<OutboundEntity> Outbounds { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<NodeEntity> Nodes { get; set; }
    public DbSet<CertificateEntity> Certificates { get; set; }
    public DbSet<GeoResourceEntity> GeoResources { get; set; }
    public DbSet<RoutingRuleEntity> RoutingRules { get; set; }
    public DbSet<AppSettingsEntity> AppSettings { get; set; }
    public DbSet<AppWebhookEntity> AppWebhooks { get; set; }
    public DbSet<ApplicationEntity> Applications { get; set; }
    public DbSet<ConnectionEntity> Connections { get; set; }
    public DbSet<WarehouseEntity> Warehouses { get; set; }
    public DbSet<ImageEntity> Images { get; set; }
    public DbSet<OperationSystemEntity> OperationSystems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureXRaynePostgresEnums();

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
                .HasDefaultValue(true);
        });

        modelBuilder.Entity<OutboundEntity>(builder =>
        {
            builder.Property(x => x.Enabled)
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

            builder.Property(x => x.CertificateMode)
                .HasDefaultValueSql("'domain'::certificate_mode");

            builder.Property(x => x.Enabled)
                .HasDefaultValue(true);
        });

        modelBuilder.Entity<CertificateEntity>(builder =>
        {
            builder.HasOne(x => x.Node)
                .WithMany(x => x.Certificates)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GeoResourceEntity>(builder =>
        {
            builder.HasOne(x => x.Node)
                .WithMany(x => x.GeoResources)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.Status)
                .HasDefaultValueSql("'success'::geo_resource_status");
        });

        modelBuilder.Entity<ApplicationEntity>(builder =>
        {
            builder.HasOne(x => x.Image)
                .WithMany()
                .HasForeignKey(x => x.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.OperationSystems)
                .WithMany(x => x.Applications)
                .UsingEntity("ApplicationOperationSystems");
        });

        modelBuilder.Entity<ImageEntity>(builder =>
        {
            builder.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Images_Version_Min", "\"Version\" >= 1");
                table.HasCheckConstraint("CK_Images_ContentType_Allowed", "\"ContentType\" IN ('image/png', 'image/jpeg', 'image/webp', 'image/gif')");
            });

            builder.Property(x => x.Version)
                .HasDefaultValue(1L);
        });

        modelBuilder.Entity<ConnectionEntity>(builder =>
        {
            builder.HasOne(x => x.User)
                .WithMany(x => x.Connections)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Application)
                .WithMany(x => x.Connections)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WarehouseEntity>(builder =>
        {
            builder.Property(x => x.Enabled)
                .HasDefaultValue(true);

            builder.HasOne(x => x.Admin)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Inbounds)
                .WithMany()
                .UsingEntity("WarehouseInbounds");
        });

        modelBuilder.Entity<OperationSystemEntity>(builder =>
        {
            builder.Property(x => x.Enabled)
                .HasDefaultValue(true);

            builder.Property(x => x.Note)
                .HasDefaultValue("");

            builder.HasOne(x => x.Image)
                .WithMany()
                .HasForeignKey(x => x.ImageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserEntity>(builder =>
        {
            builder.Property(x => x.DataLimit)
                .HasConversion(
                    value => (decimal)value,
                    value => (ulong)value);

            builder.Property(x => x.ConnectionLimit)
                .HasDefaultValue(1U);

            builder.HasOne(x => x.Admin)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Warehouse)
                .WithMany(x => x.Users)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AdminAccountEntity>(builder =>
        {
            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);
        });

        modelBuilder.Entity<AppSettingsEntity>(builder =>
        {
            builder.Property(x => x.SubscriptionProfileTitle)
                .HasDefaultValue("XRayne");

            builder.Property(x => x.SubscriptionUpdateIntervalHours)
                .HasDefaultValue(24);

            builder.HasMany(x => x.Webhooks)
                .WithOne(x => x.AppSettings)
                .HasForeignKey(x => x.AppSettingsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppWebhookEntity>(builder =>
        {
            builder.Property(x => x.Events)
                .HasConversion(
                    value => (decimal)value,
                    value => (ulong)value);

            builder.Property(x => x.RetryAttempts)
                .HasDefaultValue(3);

            builder.Property(x => x.RetryIntervalSeconds)
                .HasDefaultValue(60);
        });
    }

}
