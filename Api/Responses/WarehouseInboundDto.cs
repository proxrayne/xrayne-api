using Xray.Config.Enums;

namespace Api.Responses;

/// <summary>
/// Describes an inbound option used by warehouses.
/// </summary>
public sealed record WarehouseInboundDto(
    long Id,
    string Tag,
    string Port,
    Protocol Protocol,
    StreamNetwork? Network,
    StreamSecurity? Security,
    bool Enabled,
    long NodeId,
    string NodeName);
