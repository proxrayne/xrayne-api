using AutoMapper;
using Api.Mapping;
using Api.Requests;
using Api.Responses;
using Contracts.Configurations;
using Contracts.Enums;

namespace Test.Mapping;

public sealed class SettingsMappingProfileTests
{
    private readonly IMapper _mapper = new MapperConfiguration(
        cfg => cfg.AddProfile<SettingsMappingProfile>()).CreateMapper();

    [Fact]
    public void MapperConfiguration_IsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<SettingsMappingProfile>());

        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void MapsWebhookFlagsToEventArray()
    {
        var dto = _mapper.Map<AppWebhookDto>(new AppWebhook
        {
            Url = "https://example.com/webhook",
            Events = WebhookEvent.UserCreated | WebhookEvent.DeviceConnected,
            Secret = "secret",
        });

        dto.Events.Should().BeEquivalentTo(
            [WebhookEvent.UserCreated, WebhookEvent.DeviceConnected]);
        dto.HasSecret.Should().BeTrue();
    }

    [Fact]
    public void MapsWebhookEventArrayToFlags()
    {
        var webhook = _mapper.Map<AppWebhook>(new CreateAppWebhookRequest
        {
            Url = "https://example.com/webhook",
            Events = [WebhookEvent.UserUpdated, WebhookEvent.TrafficReset],
            Secret = "secret",
            RetryAttempts = 3,
            RetryIntervalSeconds = 60,
            SubscriptionExpirationThresholdHours = [],
            TrafficThresholdPercents = [],
        });

        webhook.Events.Should().Be(WebhookEvent.UserUpdated | WebhookEvent.TrafficReset);
        webhook.Secret.Should().Be("secret");
    }

    [Fact]
    public void MapsUpdateWebhookWithoutSecret()
    {
        var webhook = _mapper.Map<AppWebhook>(new UpdateAppWebhookRequest
        {
            Url = "https://example.com/webhook",
            Events = [WebhookEvent.UserDeleted],
            RetryAttempts = 1,
            RetryIntervalSeconds = 10,
            SubscriptionExpirationThresholdHours = [24],
            TrafficThresholdPercents = [80],
        });

        webhook.Secret.Should().BeNull();
        webhook.Events.Should().Be(WebhookEvent.UserDeleted);
    }
}
