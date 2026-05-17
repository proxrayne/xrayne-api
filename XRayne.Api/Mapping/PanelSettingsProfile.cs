using System.Reflection;
using AutoMapper;
using XRayne.Api.Requests;
using XRayne.Api.Responses;
using XRayne.Contracts.Configurations;
using PanelSettingsEntity = XRayne.Repositories.Entities.PanelSettings;

namespace XRayne.Api.Mapping;

public sealed class PanelSettingsProfile : Profile
{
    public PanelSettingsProfile()
    {
        CreateMap<PanelSettingsEntity, PanelOptions>();

        CreateMap<UpdatePanelSettingsRequest, PanelOptions>();

        CreateMap<PanelOptions, PanelSettingsResponse>()
            .ForMember(d => d.PendingRestart, o => o.Ignore())
            .ForMember(d => d.FieldImpacts, o => o.MapFrom(_ => BuildImpactMap()));
    }

    private static Dictionary<string, RestartImpact> BuildImpactMap()
    {
        var result = new Dictionary<string, RestartImpact>();
        foreach (var property in typeof(PanelOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || !property.CanWrite)
            {
                continue;
            }

            var impact = property.GetCustomAttribute<RestartImpactAttribute>()?.Impact ?? RestartImpact.None;
            result[Camelize(property.Name)] = impact;
        }

        return result;
    }

    private static string Camelize(string name) =>
        name.Length == 0 ? name : char.ToLowerInvariant(name[0]) + name[1..];
}
