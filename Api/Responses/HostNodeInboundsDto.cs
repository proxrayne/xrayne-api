namespace Api.Responses;

/// <summary>
/// Describes host inbound options grouped by node.
/// </summary>
public sealed record HostNodeInboundsDto(
    long NodeId,
    string NodeName,
    IReadOnlyList<HostInboundDto> Inbounds);
