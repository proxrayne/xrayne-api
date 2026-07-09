namespace Api.Responses;

/// <summary>
/// Describes a warehouse with assigned inbounds.
/// </summary>
public sealed record WarehouseDto(
    long Id,
    string Name,
    string Note,
    bool Enabled,
    IReadOnlyList<WarehouseInboundDto> Inbounds,
    int UsersCount);
