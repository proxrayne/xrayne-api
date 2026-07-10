namespace Api.Responses;

/// <summary>
/// Describes a warehouse option for user assignment.
/// </summary>
public sealed record UserWarehouseOptionDto(
    long Id,
    string Name,
    bool Enabled);
