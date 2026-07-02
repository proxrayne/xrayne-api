using AutoMapper;
using XRayne.Api.Requests;
using XRayne.Api.Responses;
using XRayne.Contracts.Configurations;
using XRayne.Contracts.Enums;

namespace XRayne.Api.Mapping;

/// <summary>
/// Maps application settings API models.
/// </summary>
public sealed class SettingsMappingProfile : Profile
{
    /// <summary>
    /// Creates settings API mapping rules.
    /// </summary>
    public SettingsMappingProfile()
    {
        CreateMap<AppSettings, AppSettingsResponse>()
            .ForMember(destination => destination.Webhooks, options => options.MapFrom(source => source.Webhooks));
        CreateMap<AppSettings, AppSubscriptionSettingsDto>();
        CreateMap<AppSubscriptionSettingsDto, AppSettings>()
            .ForMember(destination => destination.Webhooks, options => options.Ignore());

        CreateMap<AppWebhook, AppWebhookDto>()
            .ForMember(
                destination => destination.HasSecret,
                options => options.MapFrom(source => !string.IsNullOrWhiteSpace(source.Secret)))
            .ForMember(destination => destination.Events, options => options.MapFrom(source => Split(source.Events)));
        CreateMap<CreateAppWebhookRequest, AppWebhook>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Events, options => options.MapFrom(source => Combine(source.Events)));
        CreateMap<UpdateAppWebhookRequest, AppWebhook>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.Secret, options => options.Ignore())
            .ForMember(destination => destination.Events, options => options.MapFrom(source => Combine(source.Events)));
    }

    private static WebhookEvent Combine(IEnumerable<WebhookEvent> events) =>
        events.Aggregate(WebhookEvent.None, (current, next) => current | next);

    private static WebhookEvent[] Split(WebhookEvent events) =>
        Enum.GetValues<WebhookEvent>()
            .Where(value => value != WebhookEvent.None && events.HasFlag(value))
            .ToArray();
}
