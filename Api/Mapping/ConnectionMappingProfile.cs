using Api.Responses;
using AutoMapper;
using Data.Entities;

namespace Api.Mapping;

/// <summary>
/// Maps user connection API models.
/// </summary>
public sealed class ConnectionMappingProfile : Profile
{
    /// <summary>
    /// Creates user connection API mapping rules.
    /// </summary>
    public ConnectionMappingProfile()
    {
        CreateMap<ConnectionEntity, ConnectionDto>();
    }
}
