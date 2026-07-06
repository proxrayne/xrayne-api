using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Contracts.Configurations;
using Contracts.Enums;
using Contracts.Models;
using Infrastructure.Mapping;
using Infrastructure.Services;
using Data.Contracts;
using Data.Entities;

namespace Test.Infrastructure;

public sealed class AppSettingsServiceTests
{
    private readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<AppSettingsMappingProfile>()).CreateMapper();

    [Fact]
    public async Task GetAsync_MemoizesRepositoryResult()
    {
        var repository = Substitute.For<IAppSettingsRepository>();
        repository.GetAsync(Arg.Any<CancellationToken>())
            .Returns(new AppSettingsEntity());
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AppSettingsService(repository, cache, _mapper);

        var first = await service.GetAsync();
        var second = await service.GetAsync();

        first.Should().NotBeNull();
        second.Should().NotBeNull();
        await repository.Received(1).GetAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAsync_ReturnsClones()
    {
        var repository = Substitute.For<IAppSettingsRepository>();
        repository.GetAsync(Arg.Any<CancellationToken>())
            .Returns(new AppSettingsEntity
            {
                SubscriptionProfileTitle = "Initial",
                Announce = new SubscriptionAnnounce
                {
                    Message = "Hello",
                    Url = "https://example.com/news",
                },
                Webhooks =
                [
                    new AppWebhookSettingsEntity
                    {
                        Url = "https://example.com/webhook",
                        TrafficThresholdPercents = [50],
                    }
                ],
            });
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AppSettingsService(repository, cache, _mapper);

        var first = await service.GetAsync();
        first.SubscriptionProfileTitle = "Changed";
        first.Announce!.Message = "Changed";
        first.Webhooks[0].TrafficThresholdPercents.Add(90);

        var second = await service.GetAsync();

        second.SubscriptionProfileTitle.Should().Be("Initial");
        second.Announce!.Message.Should().Be("Hello");
        second.Webhooks[0].TrafficThresholdPercents.Should().Equal(50);
    }

    [Fact]
    public async Task UpdateAsync_NormalizesPersistsAndRefreshesCache()
    {
        var repository = Substitute.For<IAppSettingsRepository>();
        repository.GetAsync(Arg.Any<CancellationToken>())
            .Returns(new AppSettingsEntity());
        repository.UpdateAsync(Arg.Any<AppSettingsEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<AppSettingsEntity>());
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AppSettingsService(repository, cache, _mapper);

        await service.GetAsync();
        var updated = await service.UpdateAsync(new AppSettings
        {
            SubscriptionProfileTitle = "  ",
            SubscriptionSupportUrl = "  ",
            SubscriptionWebsiteUrl = " https://subscriptions.example.com ",
            SubscriptionUpdateIntervalHours = 0,
            Announce = new SubscriptionAnnounce
            {
                Message = "  Maintenance soon  ",
                Url = " https://status.example.com ",
            },
        });

        updated.SubscriptionProfileTitle.Should().Be("XRayne");
        updated.SubscriptionSupportUrl.Should().BeNull();
        updated.SubscriptionWebsiteUrl.Should().Be("https://subscriptions.example.com");
        updated.SubscriptionUpdateIntervalHours.Should().Be(1);
        updated.Announce.Should().NotBeNull();
        updated.Announce!.Message.Should().Be("Maintenance soon");
        updated.Announce.Url.Should().Be("https://status.example.com");
        updated.Webhooks.Should().BeEmpty();
        await repository.Received(1).UpdateAsync(
            Arg.Is<AppSettingsEntity>(entity =>
                entity.SubscriptionProfileTitle == "XRayne"
                && entity.SubscriptionWebsiteUrl == "https://subscriptions.example.com"
                && entity.Announce != null
                && entity.Announce.Message == "Maintenance soon"),
            Arg.Any<CancellationToken>());
        await repository.Received(1).GetAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void MapperConfiguration_IsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AppSettingsMappingProfile>());

        config.AssertConfigurationIsValid();
    }

    [Fact]
    public async Task UpdateAsync_RemovesEmptyAnnounce()
    {
        var repository = Substitute.For<IAppSettingsRepository>();
        repository.UpdateAsync(Arg.Any<AppSettingsEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<AppSettingsEntity>());
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AppSettingsService(repository, cache, _mapper);

        var updated = await service.UpdateAsync(new AppSettings
        {
            Announce = new SubscriptionAnnounce
            {
                Message = " ",
                Url = "",
            },
        });

        updated.Announce.Should().BeNull();
        await repository.Received(1).UpdateAsync(
            Arg.Is<AppSettingsEntity>(entity => entity.Announce == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSubscriptionAsync_PreservesWebhooks()
    {
        var webhookId = Guid.NewGuid();
        var repository = Substitute.For<IAppSettingsRepository>();
        repository.GetAsync(Arg.Any<CancellationToken>())
            .Returns(new AppSettingsEntity
            {
                Webhooks =
                [
                    new AppWebhookSettingsEntity
                    {
                        Id = webhookId,
                        Url = "https://example.com/webhook",
                        Secret = "stored",
                    },
                ],
            });
        repository.UpdateAsync(Arg.Any<AppSettingsEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<AppSettingsEntity>());
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AppSettingsService(repository, cache, _mapper);

        var updated = await service.UpdateSubscriptionAsync(new AppSettings
        {
            SubscriptionProfileTitle = "Subscription",
            SubscriptionUpdateIntervalHours = 8,
        });

        updated.SubscriptionProfileTitle.Should().Be("Subscription");
        updated.Webhooks.Should().ContainSingle();
        updated.Webhooks[0].Id.Should().Be(webhookId);
        updated.Webhooks[0].Secret.Should().Be("stored");
    }

    [Fact]
    public async Task AddWebhookAsync_AddsWebhookAndRefreshesCache()
    {
        var webhookId = Guid.NewGuid();
        var repository = Substitute.For<IAppSettingsRepository>();
        repository.GetAsync(Arg.Any<CancellationToken>())
            .Returns(new AppSettingsEntity
                {
                    Webhooks =
                    [
                        new AppWebhookSettingsEntity
                        {
                            Id = webhookId,
                            Url = "https://example.com/webhook",
                            Events = (ulong)WebhookEvent.UserCreated,
                            Secret = "secret",
                            RetryAttempts = 0,
                            RetryIntervalSeconds = 1,
                            TrafficThresholdPercents = [90],
                        },
                    ],
                });
        repository.AddWebhookAsync(Arg.Any<AppWebhookSettingsEntity>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<AppWebhookSettingsEntity>());
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AppSettingsService(repository, cache, _mapper);

        var created = await service.AddWebhookAsync(new AppWebhook
        {
            Id = webhookId,
            Url = " https://example.com/webhook ",
            Events = WebhookEvent.UserCreated,
            Secret = "secret",
            RetryAttempts = -1,
            RetryIntervalSeconds = 0,
            TrafficThresholdPercents = [90, 90, 101],
        });
        var cached = await service.GetAsync();

        created.Id.Should().NotBeEmpty();
        created.Url.Should().Be("https://example.com/webhook");
        created.RetryAttempts.Should().Be(0);
        created.RetryIntervalSeconds.Should().Be(1);
        created.TrafficThresholdPercents.Should().Equal(90);
        cached.Webhooks.Should().ContainSingle(item => item.Id == created.Id);
        await repository.Received(1).AddWebhookAsync(
            Arg.Is<AppWebhookSettingsEntity>(entity =>
                entity.Url == "https://example.com/webhook"
                && entity.Events == (ulong)WebhookEvent.UserCreated
                && entity.RetryAttempts == 0
                && entity.RetryIntervalSeconds == 1
                && entity.TrafficThresholdPercents.SequenceEqual(new[] { 90 })),
            Arg.Any<CancellationToken>());
        await repository.Received(1).GetAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateWebhookAsync_PreservesExistingSecret()
    {
        var webhookId = Guid.NewGuid();
        var repository = Substitute.For<IAppSettingsRepository>();
        repository.GetAsync(Arg.Any<CancellationToken>())
            .Returns(
                new AppSettingsEntity
                {
                    Webhooks =
                    [
                        new AppWebhookSettingsEntity
                        {
                            Id = webhookId,
                            Url = "https://example.com/updated",
                            Secret = "stored",
                            Events = (ulong)WebhookEvent.UserDeleted,
                        },
                    ],
                });
        repository.UpdateWebhookAsync(
                webhookId,
                Arg.Any<AppWebhookSettingsEntity>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var entity = call.Arg<AppWebhookSettingsEntity>();
                entity.Id = webhookId;
                entity.Secret = "stored";

                return entity;
            });
        string? persistedSecret = null;
        ulong? persistedEvents = null;
        repository.UpdateWebhookAsync(
                webhookId,
                Arg.Do<AppWebhookSettingsEntity>(entity =>
                {
                    persistedSecret = entity.Secret;
                    persistedEvents = entity.Events;
                }),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var entity = call.Arg<AppWebhookSettingsEntity>();
                entity.Id = webhookId;
                entity.Secret = "stored";

                return entity;
            });
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AppSettingsService(repository, cache, _mapper);

        var updated = await service.UpdateWebhookAsync(webhookId, new AppWebhook
        {
            Url = "https://example.com/updated",
            Events = WebhookEvent.UserDeleted,
            Secret = "ignored",
            RetryAttempts = 5,
            RetryIntervalSeconds = 15,
        });

        updated.Should().NotBeNull();
        updated!.Id.Should().Be(webhookId);
        updated.Secret.Should().Be("stored");
        updated.Events.Should().Be(WebhookEvent.UserDeleted);
        persistedSecret.Should().BeNull();
        persistedEvents.Should().Be((ulong)WebhookEvent.UserDeleted);
        await repository.Received(1).UpdateWebhookAsync(
            webhookId,
            Arg.Any<AppWebhookSettingsEntity>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateWebhookAsync_ReturnsNull_WhenWebhookMissing()
    {
        var repository = Substitute.For<IAppSettingsRepository>();
        repository.UpdateWebhookAsync(
                Arg.Any<Guid>(),
                Arg.Any<AppWebhookSettingsEntity>(),
                Arg.Any<CancellationToken>())
            .Returns((AppWebhookSettingsEntity?)null);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AppSettingsService(repository, cache, _mapper);

        var updated = await service.UpdateWebhookAsync(Guid.NewGuid(), new AppWebhook
        {
            Url = "https://example.com/webhook",
        });

        updated.Should().BeNull();
        await repository.DidNotReceive().GetAsync(Arg.Any<CancellationToken>());
        await repository.Received(1).UpdateWebhookAsync(
            Arg.Any<Guid>(),
            Arg.Any<AppWebhookSettingsEntity>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteWebhookAsync_RemovesWebhookAndRefreshesCache()
    {
        var webhookId = Guid.NewGuid();
        var repository = Substitute.For<IAppSettingsRepository>();
        repository.GetAsync(Arg.Any<CancellationToken>())
            .Returns(new AppSettingsEntity());
        repository.DeleteWebhookAsync(webhookId, Arg.Any<CancellationToken>())
            .Returns(true);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AppSettingsService(repository, cache, _mapper);

        var deleted = await service.DeleteWebhookAsync(webhookId);
        var cached = await service.GetAsync();

        deleted.Should().BeTrue();
        cached.Webhooks.Should().BeEmpty();
        await repository.Received(1).GetAsync(Arg.Any<CancellationToken>());
        await repository.Received(1).DeleteWebhookAsync(webhookId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteWebhookAsync_ReturnsFalse_WhenWebhookMissing()
    {
        var repository = Substitute.For<IAppSettingsRepository>();
        repository.DeleteWebhookAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AppSettingsService(repository, cache, _mapper);

        var deleted = await service.DeleteWebhookAsync(Guid.NewGuid());

        deleted.Should().BeFalse();
        await repository.DidNotReceive().GetAsync(Arg.Any<CancellationToken>());
        await repository.Received(1).DeleteWebhookAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
