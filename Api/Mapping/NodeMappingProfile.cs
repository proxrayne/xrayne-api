using AutoMapper;
using XRayne.Api.Responses;
using XRayne.Repositories.Entities;

namespace XRayne.Api.Mapping;

/// <summary>
/// Maps remote node API models.
/// </summary>
public sealed class NodeMappingProfile : Profile
{
    /// <summary>
    /// Creates remote node API mapping rules.
    /// </summary>
    public NodeMappingProfile()
    {
        CreateMap<NodeEntity, NodeDto>();
    }
}
