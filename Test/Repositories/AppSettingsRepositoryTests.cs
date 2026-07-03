using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Entities;
using Repositories.Implementations;

namespace Test.Repositories;

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
        var entity = context.Model.FindEntityType(typeof(AppWebhookSettingsEntity));

        entity.Should().NotBeNull();
        entity!.FindProperty(nameof(AppWebhookSettingsEntity.Events))!
            .GetColumnType()
            .Should()
            .Be("numeric(20,0)");
        entity.FindProperty(nameof(AppWebhookSettingsEntity.SubscriptionExpirationThresholdHours))!
            .GetColumnType()
            .Should()
            .Be("jsonb");
        entity.FindProperty(nameof(AppWebhookSettingsEntity.TrafficThresholdPercents))!
            .GetColumnType()
            .Should()
            .Be("jsonb");
        entity.FindNavigation(nameof(AppWebhookSettingsEntity.AppSettings))!
            .ForeignKey
            .DeleteBehavior
            .Should()
            .Be(DeleteBehavior.Cascade);
    }

    private static AppDbContext CreateNpgsqlModelContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=xrayne;Username=xrayne;Password=xrayne")
            .Options;

        return new AppDbContext(options);
    }
}
