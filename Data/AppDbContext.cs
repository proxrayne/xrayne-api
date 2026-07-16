using Microsoft.EntityFrameworkCore;
using Data.Entities;

namespace Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AdminAccount> AdminAccounts { get; set; }
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

            builder.Property(x => x.CertificateMode)
                .HasColumnType("certificate_mode")
                .HasDefaultValueSql("'domain'::certificate_mode");

            builder.Property(x => x.Enabled)
                .HasDefaultValue(true);

            builder.Property(x => x.AuthType)
                .HasColumnType("ssh_auth_type");

            builder.Property(x => x.ConfigTemplate)
                .HasColumnType("jsonb");
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
                .WithMany(x => x.GeoResources)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex("NodeId", nameof(GeoResourceEntity.Filename))
                .IsUnique();

            builder.Property(x => x.Status)
                .HasColumnType("geo_resource_status")
                .HasDefaultValueSql("'success'::geo_resource_status");
        });

        modelBuilder.Entity<ApplicationEntity>(builder =>
        {
            builder.Property(x => x.Assets)
                .HasColumnType("jsonb");

            builder.Property(x => x.SubscriptionFormat)
                .HasColumnType("subscription_format");

            builder.HasOne(x => x.Admin)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConnectionEntity>(builder =>
        {
            builder.Property(x => x.Flow)
                .HasColumnType("xtls_flow");

            builder.Property(x => x.Method)
                .HasColumnType("encryption_method");

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

        modelBuilder.Entity<UserEntity>(builder =>
        {
            builder.Property(x => x.Status)
                .HasColumnType("user_status");

            builder.Property(x => x.LimitResetStrategy)
                .HasColumnType("limit_reset_strategy");

            builder.Property(x => x.DataLimit)
                .HasColumnType("numeric(20,0)")
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

        modelBuilder.Entity<AdminAccount>(builder =>
        {
            builder.Property(x => x.Permissions)
                .HasColumnType("bigint");

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);
        });

        modelBuilder.Entity<AppSettingsEntity>(builder =>
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.SubscriptionProfileTitle)
                .IsRequired()
                .HasMaxLength(256)
                .HasDefaultValue("XRayne");

            builder.Property(x => x.SubscriptionSupportUrl)
                .HasMaxLength(2048);

            builder.Property(x => x.SubscriptionWebsiteUrl)
                .HasMaxLength(2048);

            builder.Property(x => x.SubscriptionUpdateIntervalHours)
                .IsRequired()
                .HasDefaultValue(24);

            builder.Property(x => x.Announce)
                .HasColumnType("jsonb");

            builder.HasMany(x => x.Webhooks)
                .WithOne(x => x.AppSettings)
                .HasForeignKey(x => x.AppSettingsId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppWebhookEntity>(builder =>
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Url)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(x => x.Events)
                .HasColumnType("numeric(20,0)")
                .HasConversion(
                    value => (decimal)value,
                    value => (ulong)value);

            builder.Property(x => x.Secret)
                .HasMaxLength(1024);

            builder.Property(x => x.RetryAttempts)
                .IsRequired()
                .HasDefaultValue(3);

            builder.Property(x => x.RetryIntervalSeconds)
                .IsRequired()
                .HasDefaultValue(60);

            builder.Property(x => x.SubscriptionExpirationThresholdHours)
                .HasColumnType("jsonb");

            builder.Property(x => x.TrafficThresholdPercents)
                .HasColumnType("jsonb");
        });
    }

}
