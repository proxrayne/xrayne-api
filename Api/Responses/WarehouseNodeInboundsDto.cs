namespace Api.Responses;

/// <summary>
/// Describes warehouse inbound options grouped by node.
/// </summary>
public sealed record WarehouseNodeInboundsDto(
    long NodeId,
    string NodeName,
    IReadOnlyList<WarehouseInboundDto> Inbounds);
