using Api.Responses;
using AutoMapper;
using Contracts.Models;
using Contracts.Utilities;
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

        CreateMap<InboundEntity, NodeInboundDto>()
            .ForCtorParam(
                nameof(NodeInboundDto.Id),
                options => options.MapFrom(inbound => inbound.Id))
            .ForCtorParam(
                nameof(NodeInboundDto.Port),
                options => options.MapFrom(inbound => inbound.Port.ToString()))
            .ForCtorParam(
                nameof(NodeInboundDto.Config),
                options => options.MapFrom(inbound => XrayJsonSerializer.Serialize(inbound.Config)));

        CreateMap<InboundEntity, NodeInboundListItemDto>()
            .ForCtorParam(
                nameof(NodeInboundListItemDto.Id),
                options => options.MapFrom(inbound => inbound.Id))
            .ForCtorParam(
                nameof(NodeInboundListItemDto.Port),
                options => options.MapFrom(inbound => inbound.Port.ToString()));

        CreateMap<OutboundEntity, NodeOutboundDto>()
            .ForCtorParam(
                nameof(NodeOutboundDto.Id),
                options => options.MapFrom(outbound => outbound.Id))
            .ForCtorParam(
                nameof(NodeOutboundDto.Tag),
                options => options.MapFrom(outbound => outbound.Tag ?? string.Empty))
            .ForCtorParam(
                nameof(NodeOutboundDto.Config),
                options => options.MapFrom(outbound => XrayJsonSerializer.Serialize(outbound.Config)));

        CreateMap<OutboundEntity, NodeOutboundListItemDto>()
            .ForCtorParam(
                nameof(NodeOutboundListItemDto.Id),
                options => options.MapFrom(outbound => outbound.Id))
            .ForCtorParam(
                nameof(NodeOutboundListItemDto.Tag),
                options => options.MapFrom(outbound => outbound.Tag ?? string.Empty));

        CreateMap<RoutingRuleEntity, NodeRoutingRuleDto>()
            .ForCtorParam(
                nameof(NodeRoutingRuleDto.Id),
                options => options.MapFrom(rule => rule.Id))
            .ForCtorParam(
                nameof(NodeRoutingRuleDto.Tag),
                options => options.MapFrom(rule => rule.RuleTag ?? string.Empty))
            .ForCtorParam(
                nameof(NodeRoutingRuleDto.Config),
                options => options.MapFrom(rule => XrayJsonSerializer.Serialize(rule.Config)));

        CreateMap<RoutingRuleEntity, NodeRoutingRuleListItemDto>()
            .ForCtorParam(
                nameof(NodeRoutingRuleListItemDto.Id),
                options => options.MapFrom(rule => rule.Id))
            .ForCtorParam(
                nameof(NodeRoutingRuleListItemDto.Tag),
                options => options.MapFrom(rule => rule.RuleTag ?? string.Empty));

        CreateMap<CertificateEntity, NodeCertificateDto>();

        CreateMap<GeoResourceEntity, NodeGeoResourceDto>()
            .ForCtorParam(
                nameof(NodeGeoResourceDto.FileName),
                options => options.MapFrom(resource => resource.Filename));
    }
}
