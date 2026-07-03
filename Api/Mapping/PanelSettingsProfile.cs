using AutoMapper;
using Api.Requests;
using Api.Responses;
using Contracts.Configurations;

namespace Api.Mapping;

public sealed class PanelSettingsProfile : Profile
{
    public PanelSettingsProfile()
    {
        CreateMap<PanelSettings, PanelSettingsDto>();
        CreateMap<UpdatePanelSettingsRequest, PanelSettings>();
        CreateMap<PanelSettings, PanelSettingsResponse>();
    }
}
