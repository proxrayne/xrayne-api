using AutoMapper;
using Api.Requests;
using Api.Responses;
using Contracts.Configurations;

namespace Api.Mapping;

/// <summary>
/// Configures mappings for panel bootstrap settings request and response models.
/// </summary>
public sealed class PanelSettingsProfile : Profile
{
    /// <summary>
    /// Initializes panel settings mappings.
    /// </summary>
    public PanelSettingsProfile()
    {
        CreateMap<PanelSettings, PanelSettingsDto>()
            .ForMember(response => response.PathBase, options => options.Ignore())
            .ForMember(response => response.SessionLifetimeMinutes, options => options.Ignore());
        CreateMap<UpdatePanelSettingsRequest, PanelSettings>();
        CreateMap<PanelSettings, PanelSettingsResponse>()
            .ForMember(response => response.Settings, options => options.MapFrom(settings => settings))
            .ForMember(response => response.PendingRestart, options => options.Ignore());
    }
}
