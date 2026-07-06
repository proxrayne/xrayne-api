using Api.Responses;
using AutoMapper;
using Contracts.Models;
using Data.Entities;

namespace Api.Mapping;

/// <summary>
/// Maps remote node API models.
/// </summary>
public sealed class NodeMappingProfile : Profile
{
    /// <summary>
    /// Mapping context key that contains the current node connection state.
    /// </summary>
    public const string ConnectionStateItemKey = "NodeConnectionState";

    /// <summary>
    /// Creates remote node API mapping rules.
    /// </summary>
    public NodeMappingProfile()
    {
        CreateMap<NodeEntity, NodeDto>()
            .ForCtorParam(
                nameof(NodeDto.Status),
                options => options.MapFrom((_, context) =>
                    ((NodeConnectionState)context.Items[ConnectionStateItemKey]).Status));
    }
}
