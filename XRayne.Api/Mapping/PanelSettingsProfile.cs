using AutoMapper;
using XRayne.Api.Requests;
using XRayne.Api.Responses;
using XRayne.Contracts.Configurations;

namespace XRayne.Api.Mapping;

public sealed class PanelSettingsProfile : Profile
{
    public PanelSettingsProfile()
    {
        CreateMap<PanelSettings, PanelSettingsDto>();
        CreateMap<UpdatePanelSettingsRequest, PanelSettings>();
        CreateMap<PanelSettings, PanelSettingsResponse>();
    }
}
