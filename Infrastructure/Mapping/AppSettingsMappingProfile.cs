using AutoMapper;
using Contracts.Configurations;
using Contracts.Enums;
using Contracts.Models;
using Repositories.Entities;

namespace Infrastructure.Mapping;

/// <summary>
/// Maps database-backed application settings models.
/// </summary>
public sealed class AppSettingsMappingProfile : Profile
{
    /// <summary>
    /// Creates application settings mapping rules.
    /// </summary>
    public AppSettingsMappingProfile()
    {
        CreateMap<AppSettingsEntity, AppSettings>();
        CreateMap<AppWebhookSettingsEntity, AppWebhook>()
            .ForMember(
                destination => destination.Events,
                options => options.MapFrom(source => (WebhookEvent)source.Events));

        CreateMap<AppSettings, AppSettingsEntity>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore());
        CreateMap<AppWebhook, AppWebhookSettingsEntity>()
            .ForMember(destination => destination.AppSettingsId, options => options.Ignore())
            .ForMember(destination => destination.AppSettings, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(
                destination => destination.Events,
                options => options.MapFrom(source => (ulong)source.Events));

        CreateMap<AppSettings, AppSettings>();
        CreateMap<SubscriptionAnnounce, SubscriptionAnnounce>();
        CreateMap<AppWebhook, AppWebhook>();
    }
}
