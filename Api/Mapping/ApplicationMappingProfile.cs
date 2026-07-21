using Api.Requests;
using Api.Responses;
using AutoMapper;
using Data.Entities;
using Data.Models;
using OptionalValues;

namespace Api.Mapping;

/// <summary>
/// Maps application management API models.
/// </summary>
public sealed class ApplicationMappingProfile : Profile
{
    /// <summary>
    /// Creates application management API mapping rules.
    /// </summary>
    public ApplicationMappingProfile()
    {
        CreateMap<ImageEntity, ImageDto>();
        CreateMap<OperationSystemEntity, OperationSystemDto>();
        CreateMap<ApplicationEntity, ApplicationDto>();
        CreateMap<PatchApplicationRequest, ApplicationPatch>()
            .ForMember(
                target => target.Assets,
                options => options.ConvertUsing(
                    new OptionalCollectionConverter<string>(),
                    source => source.Assets))
            .ForMember(
                target => target.OperationSystemIds,
                options => options.ConvertUsing(
                    new OptionalCollectionConverter<string>(),
                    source => source.OperationSystemIds));
        CreateMap<PatchOperationSystemRequest, OperationSystemPatch>();
    }

    private sealed class OptionalCollectionConverter<T>
        : IValueConverter<OptionalValue<List<T>?>, OptionalValue<IReadOnlyCollection<T>?>>
    {
        public OptionalValue<IReadOnlyCollection<T>?> Convert(
            OptionalValue<List<T>?> source,
            ResolutionContext context)
        {
            return source.IsSpecified
                ? source.SpecifiedValue
                : OptionalValue<IReadOnlyCollection<T>?>.Unspecified;
        }
    }
}
