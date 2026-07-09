using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Contracts.Enums;
using Data.Entities;
using Xray.Config.Enums;

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
    public DbSet<AppWebhookEntity> AppWebhookSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<UserStatus>();
        modelBuilder.HasPostgresEnum<LimitResetStrategy>();
        modelBuilder.HasPostgresEnum<AdminPermission>();
        modelBuilder.HasPostgresEnum<SSHAuthType>();
        modelBuilder.HasPostgresEnum<CertificateMode>();
        modelBuilder.HasPostgresEnum<GeoResourceSourceType>();
        modelBuilder.HasPostgresEnum<XtlsFlow>();
        modelBuilder.HasPostgresEnum<EncryptionMethod>();
        modelBuilder.HasPostgresEnum<SubscriptionFormat>();

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
                .HasDefaultValue(CertificateMode.Domain);

            builder.Property(x => x.Enabled)
                .HasDefaultValue(true);

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

            var sourceType = builder.Property(x => x.SourceType)
                .HasColumnType("geo_resource_source_type")
                .HasDefaultValueSql("'static'::geo_resource_source_type");
            sourceType.Metadata.SetValueConverter((ValueConverter?)null);
            sourceType.Metadata.SetProviderClrType(null);
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

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Enum>()
            .HaveConversion<string>();
    }
}
