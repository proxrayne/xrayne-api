using Api.Responses;
using Api.Requests;
using AutoMapper;
using Contracts.Models;
using Contracts.Utilities;
using Data.Entities;
using Infrastructure.Dto;

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
    /// Mapping context key that contains the current node xray-core state.
    /// </summary>
    public const string CoreStateItemKey = "NodeCoreState";

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

        CreateMap<NodeEntity, NodeListItemDto>()
            .ForCtorParam(
                nameof(NodeListItemDto.Status),
                options => options.MapFrom((_, context) =>
                    ((NodeConnectionState)context.Items[ConnectionStateItemKey]).Status))
            .ForCtorParam(
                nameof(NodeListItemDto.NodeVersion),
                options => options.MapFrom((_, context) =>
                    ((NodeConnectionState)context.Items[ConnectionStateItemKey]).ApiVersion))
            .ForCtorParam(
                nameof(NodeListItemDto.Xray),
                options => options.MapFrom((_, context) =>
                    ToXrayDto((NodeCoreState?)context.Items[CoreStateItemKey])));

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
                options => options.MapFrom(rule => rule.RuleTag ?? string.Empty))
            .ForCtorParam(
                nameof(NodeRoutingRuleListItemDto.Config),
                options => options.MapFrom(rule => XrayJsonSerializer.Serialize(rule.Config)));

        CreateMap<SaveNodeRoutingRuleManualRequest, NodeRoutingRuleManualSaveItem>();
        CreateMap<SaveNodeRoutingRuleReadonlyRequest, NodeRoutingRuleReadonlySaveItem>();

        CreateMap<GeoResourceEntity, NodeGeoResourceDto>()
            .ForCtorParam(
                nameof(NodeGeoResourceDto.FileName),
                options => options.MapFrom(resource => resource.Filename));
    }

    private static NodeListItemXrayDto ToXrayDto(NodeCoreState? state)
    {
        return state is null
            ? new NodeListItemXrayDto(false, false, null, null)
            : new NodeListItemXrayDto(
                state.IsInstalled,
                state.IsRunning,
                state.Version,
                state.Status);
    }
}
