using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using XRayne.Api.Controllers;
using XRayne.Api.Exceptions;
using XRayne.Api.Mapping;
using XRayne.Api.Requests;
using XRayne.Api.Responses;
using XRayne.Contracts.Configurations;
using XRayne.Contracts.Enums;
using XRayne.Contracts.Models;
using XRayne.Infrastructure.Services;

namespace XRayne.Test.Controllers;

public sealed class SettingsControllerTests
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly IPanelRestartService _restartService;
    private readonly IMapper _mapper;
    private readonly SettingsController _controller;

    public SettingsControllerTests()
    {
        _appSettingsService = Substitute.For<IAppSettingsService>();
        _restartService = Substitute.For<IPanelRestartService>();
        _restartService.ScheduleRestart().Returns(true);
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<SettingsMappingProfile>()).CreateMapper();

        _controller = new SettingsController(
            _appSettingsService,
            _restartService,
            NullLogger<SettingsController>.Instance,
            _mapper)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task GetAppSettings_ReturnsSnapshotWithoutWebhookSecret()
    {
        var webhookId = Guid.NewGuid();
        _appSettingsService.GetAsync(Arg.Any<CancellationToken>())
            .Returns(new AppSettings
            {
                SubscriptionProfileTitle = "Panel",
                SubscriptionWebsiteUrl = "https://example.com",
                Announce = new SubscriptionAnnounce { Message = "Hello", Url = "https://example.com/news" },
                Webhooks =
                [
                    new AppWebhook
                    {
                        Id = webhookId,
                        Url = "https://example.com/webhook",
                        Events = WebhookEvent.UserCreated | WebhookEvent.TrafficReset,
                        Secret = "hidden",
                    },
                ],
            });

        var result = await _controller.GetAppSettings(CancellationToken.None);

        result.SubscriptionProfileTitle.Should().Be("Panel");
        result.SubscriptionWebsiteUrl.Should().Be("https://example.com");
        result.Announce!.Message.Should().Be("Hello");
        result.Webhooks.Should().ContainSingle();
        result.Webhooks[0].Id.Should().Be(webhookId);
        result.Webhooks[0].Events.Should().BeEquivalentTo(
            [WebhookEvent.UserCreated, WebhookEvent.TrafficReset]);
        typeof(AppWebhookDto).GetProperty("Secret").Should().BeNull();
    }

    [Fact]
    public async Task UpdateSubscriptionSettings_DelegatesAndReturnsSettings()
    {
        _appSettingsService.UpdateSubscriptionAsync(Arg.Any<AppSettings>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var settings = call.Arg<AppSettings>();

                return new AppSettings
                {
                    SubscriptionProfileTitle = settings.SubscriptionProfileTitle,
                    SubscriptionSupportUrl = settings.SubscriptionSupportUrl,
                    SubscriptionWebsiteUrl = settings.SubscriptionWebsiteUrl,
                    SubscriptionUpdateIntervalHours = settings.SubscriptionUpdateIntervalHours,
                    Announce = settings.Announce,
                    Webhooks =
                    [
                        new AppWebhook { Url = "https://example.com/webhook" },
                    ],
                };
            });
        var request = new AppSubscriptionSettingsDto
        {
            SubscriptionProfileTitle = "Updated",
            SubscriptionSupportUrl = "https://support.example.com",
            SubscriptionWebsiteUrl = "https://example.com",
            SubscriptionUpdateIntervalHours = 12,
            Announce = new SubscriptionAnnounce { Message = "News" },
        };

        var result = await _controller.UpdateSubscriptionSettings(request, CancellationToken.None);

        result.SubscriptionProfileTitle.Should().Be("Updated");
        result.SubscriptionUpdateIntervalHours.Should().Be(12);
        result.Webhooks.Should().ContainSingle();
        await _appSettingsService.Received(1).UpdateSubscriptionAsync(
            Arg.Is<AppSettings>(settings =>
                settings.SubscriptionProfileTitle == "Updated"
                && settings.SubscriptionSupportUrl == "https://support.example.com"
                && settings.Webhooks.Count == 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateWebhook_AcceptsSecretAndReturnsCreatedWithoutSecret()
    {
        var webhookId = Guid.NewGuid();
        _appSettingsService.AddWebhookAsync(Arg.Any<AppWebhook>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var webhook = call.Arg<AppWebhook>();
                webhook.Id = webhookId;

                return webhook;
            });
        var request = new CreateAppWebhookRequest
        {
            Url = "https://example.com/webhook",
            Events = [WebhookEvent.UserCreated, WebhookEvent.UserDeleted],
            Secret = "secret",
            RetryAttempts = 2,
            RetryIntervalSeconds = 30,
            SubscriptionExpirationThresholdHours = [24],
            TrafficThresholdPercents = [80],
        };

        var result = await _controller.CreateWebhook(request, CancellationToken.None);

        var created = result.Should().BeOfType<CreatedResult>().Subject;
        created.Location.Should().Be($"/api/settings/app/webhooks/{webhookId}");
        var body = created.Value.Should().BeOfType<AppWebhookDto>().Subject;
        body.Id.Should().Be(webhookId);
        body.Events.Should().BeEquivalentTo([WebhookEvent.UserCreated, WebhookEvent.UserDeleted]);
        typeof(AppWebhookDto).GetProperty("Secret").Should().BeNull();
        await _appSettingsService.Received(1).AddWebhookAsync(
            Arg.Is<AppWebhook>(webhook =>
                webhook.Secret == "secret"
                && webhook.Events == (WebhookEvent.UserCreated | WebhookEvent.UserDeleted)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateWebhook_UsesRequestWithoutSecret()
    {
        var webhookId = Guid.NewGuid();
        _appSettingsService.UpdateWebhookAsync(
                webhookId,
                Arg.Any<AppWebhook>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var webhook = call.Arg<AppWebhook>();

                return new AppWebhook
                {
                    Id = webhookId,
                    Url = webhook.Url,
                    Events = webhook.Events,
                    Secret = "stored",
                    RetryAttempts = webhook.RetryAttempts,
                    RetryIntervalSeconds = webhook.RetryIntervalSeconds,
                    SubscriptionExpirationThresholdHours = webhook.SubscriptionExpirationThresholdHours,
                    TrafficThresholdPercents = webhook.TrafficThresholdPercents,
                };
            });
        var request = new UpdateAppWebhookRequest
        {
            Url = "https://example.com/updated",
            Events = [WebhookEvent.DeviceConnected],
            RetryAttempts = 4,
            RetryIntervalSeconds = 15,
            SubscriptionExpirationThresholdHours = [],
            TrafficThresholdPercents = [90],
        };

        var result = await _controller.UpdateWebhook(webhookId, request, CancellationToken.None);

        result.Id.Should().Be(webhookId);
        result.Events.Should().BeEquivalentTo([WebhookEvent.DeviceConnected]);
        typeof(UpdateAppWebhookRequest).GetProperty("Secret").Should().BeNull();
        await _appSettingsService.Received(1).UpdateWebhookAsync(
            webhookId,
            Arg.Is<AppWebhook>(webhook =>
                webhook.Secret == null
                && webhook.Events == WebhookEvent.DeviceConnected),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateWebhook_ThrowsNotFound_WhenServiceReturnsNull()
    {
        _appSettingsService.UpdateWebhookAsync(
                Arg.Any<Guid>(),
                Arg.Any<AppWebhook>(),
                Arg.Any<CancellationToken>())
            .Returns((AppWebhook?)null);

        var act = () => _controller.UpdateWebhook(
            Guid.NewGuid(),
            new UpdateAppWebhookRequest
            {
                Url = "https://example.com/webhook",
                Events = [],
                RetryAttempts = 0,
                RetryIntervalSeconds = 1,
                SubscriptionExpirationThresholdHours = [],
                TrafficThresholdPercents = [],
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteWebhook_ReturnsNoContent()
    {
        var webhookId = Guid.NewGuid();
        _appSettingsService.DeleteWebhookAsync(webhookId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _controller.DeleteWebhook(webhookId, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteWebhook_ThrowsNotFound_WhenMissing()
    {
        _appSettingsService.DeleteWebhookAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var act = () => _controller.DeleteWebhook(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public void Restart_Returns202_AndSchedulesRestart()
    {
        var result = _controller.RestartPanel();

        result.Should().BeOfType<AcceptedResult>();
        _restartService.Received(1).ScheduleRestart();
    }

    [Fact]
    public void Restart_Returns202_EvenIfAlreadyScheduled()
    {
        _restartService.ScheduleRestart().Returns(false);

        var result = _controller.RestartPanel();

        result.Should().BeOfType<AcceptedResult>();
    }
}
