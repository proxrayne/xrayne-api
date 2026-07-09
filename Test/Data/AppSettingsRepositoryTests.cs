using Microsoft.EntityFrameworkCore;
using Data;
using Data.Entities;
using Data.Implementations;

namespace Test.Data;

/// <summary>
/// Tests application settings persistence.
/// </summary>
public sealed class AppSettingsRepositoryTests
{
    [Fact]
    public void Model_ConfiguresSingletonSettings()
    {
        using var context = CreateNpgsqlModelContext();
        var entity = context.Model.FindEntityType(typeof(AppSettingsEntity));

        entity.Should().NotBeNull();
        entity!.FindPrimaryKey()!.Properties.Should().ContainSingle(property => property.Name == "Id");
        entity.FindProperty(nameof(AppSettingsEntity.Id))!
            .ValueGenerated
            .Should()
            .Be(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never);
        entity.FindProperty(nameof(AppSettingsEntity.SubscriptionProfileTitle))!
            .GetDefaultValue()
            .Should()
            .Be("XRayne");
        entity.FindProperty(nameof(AppSettingsEntity.SubscriptionUpdateIntervalHours))!
            .GetDefaultValue()
            .Should()
            .Be(24);
        entity.FindProperty(nameof(AppSettingsEntity.SubscriptionWebsiteUrl))!
            .GetMaxLength()
            .Should()
            .Be(2048);
        entity.FindProperty(nameof(AppSettingsEntity.Announce))!
            .GetColumnType()
            .Should()
            .Be("jsonb");
    }

    [Fact]
    public void Model_ConfiguresWebhookPersistence()
    {
        using var context = CreateNpgsqlModelContext();
        var entity = context.Model.FindEntityType(typeof(AppWebhookEntity));

        entity.Should().NotBeNull();
        entity!.GetTableName().Should().Be("AppWebhooks");
        entity.FindProperty(nameof(AppWebhookEntity.Events))!
            .GetColumnType()
            .Should()
            .Be("numeric(20,0)");
        entity.FindProperty(nameof(AppWebhookEntity.SubscriptionExpirationThresholdHours))!
            .GetColumnType()
            .Should()
            .Be("jsonb");
        entity.FindProperty(nameof(AppWebhookEntity.TrafficThresholdPercents))!
            .GetColumnType()
            .Should()
            .Be("jsonb");
        entity.FindNavigation(nameof(AppWebhookEntity.AppSettings))!
            .ForeignKey
            .DeleteBehavior
            .Should()
            .Be(DeleteBehavior.Cascade);
    }

    [Fact]
    public void Model_ConfiguresConnectionWarehousePersistence()
    {
        using var context = CreateNpgsqlModelContext();
        var application = context.Model.FindEntityType(typeof(ApplicationEntity));
        var connection = context.Model.FindEntityType(typeof(ConnectionEntity));
        var warehouse = context.Model.FindEntityType(typeof(WarehouseEntity));
        var user = context.Model.FindEntityType(typeof(UserEntity));

        application.Should().NotBeNull();
        application!.FindProperty(nameof(ApplicationEntity.Assets))!
            .GetColumnType()
            .Should()
            .Be("jsonb");
        application.FindProperty(nameof(ApplicationEntity.SubscriptionFormat))!
            .GetColumnType()
            .Should()
            .Be("subscription_format");

        connection.Should().NotBeNull();
        connection!.FindNavigation(nameof(ConnectionEntity.User))!
            .ForeignKey
            .DeleteBehavior
            .Should()
            .Be(DeleteBehavior.Cascade);
        connection.FindNavigation(nameof(ConnectionEntity.Application))!
            .ForeignKey
            .DeleteBehavior
            .Should()
            .Be(DeleteBehavior.SetNull);
        connection.FindProperty(nameof(ConnectionEntity.Flow))!
            .GetColumnType()
            .Should()
            .Be("xtls_flow");
        connection.FindProperty(nameof(ConnectionEntity.Method))!
            .GetColumnType()
            .Should()
            .Be("encryption_method");

        warehouse.Should().NotBeNull();
        warehouse!.FindNavigation(nameof(WarehouseEntity.Users))!
            .ForeignKey
            .DeleteBehavior
            .Should()
            .Be(DeleteBehavior.Restrict);
        user!.FindProperty(nameof(UserEntity.ConnectionLimit))!
            .GetDefaultValue()
            .Should()
            .Be(1U);
    }

    [Fact]
    public void Model_ConfiguresNodeConfigTemplateAsJsonb()
    {
        using var context = CreateNpgsqlModelContext();
        var entity = context.Model.FindEntityType(typeof(NodeEntity));

        entity.Should().NotBeNull();
        entity!.FindProperty(nameof(NodeEntity.ConfigTemplate))!
            .GetColumnType()
            .Should()
            .Be("jsonb");
    }

    [Fact]
    public void Model_ConfiguresEnumsAsPostgresEnumTypes()
    {
        using var context = CreateNpgsqlModelContext();
        var admin = context.Model.FindEntityType(typeof(AdminAccount));
        var node = context.Model.FindEntityType(typeof(NodeEntity));
        var user = context.Model.FindEntityType(typeof(UserEntity));
        var geoResource = context.Model.FindEntityType(typeof(GeoResourceEntity));
        var application = context.Model.FindEntityType(typeof(ApplicationEntity));
        var connection = context.Model.FindEntityType(typeof(ConnectionEntity));

        admin!.FindProperty(nameof(AdminAccount.Permissions))!
            .GetColumnType()
            .Should()
            .Be("bigint");
        admin.FindProperty(nameof(AdminAccount.Permissions))!
            .GetValueConverter()
            .Should()
            .BeNull();
        node!.FindProperty(nameof(NodeEntity.CertificateMode))!
            .GetColumnType()
            .Should()
            .Be("certificate_mode");
        node.FindProperty(nameof(NodeEntity.AuthType))!
            .GetColumnType()
            .Should()
            .Be("ssh_auth_type");
        user!.FindProperty(nameof(UserEntity.Status))!
            .GetColumnType()
            .Should()
            .Be("user_status");
        user.FindProperty(nameof(UserEntity.LimitResetStrategy))!
            .GetColumnType()
            .Should()
            .Be("limit_reset_strategy");
        geoResource!.FindProperty(nameof(GeoResourceEntity.SourceType))!
            .GetColumnType()
            .Should()
            .Be("geo_resource_source_type");
        application!.FindProperty(nameof(ApplicationEntity.SubscriptionFormat))!
            .GetColumnType()
            .Should()
            .Be("subscription_format");
        connection!.FindProperty(nameof(ConnectionEntity.Flow))!
            .GetColumnType()
            .Should()
            .Be("xtls_flow");
        connection.FindProperty(nameof(ConnectionEntity.Method))!
            .GetColumnType()
            .Should()
            .Be("encryption_method");
    }

    private static AppDbContext CreateNpgsqlModelContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseXRayneNpgsql("Host=localhost;Database=xrayne;Username=xrayne;Password=xrayne")
            .Options;

        return new AppDbContext(options);
    }
}
