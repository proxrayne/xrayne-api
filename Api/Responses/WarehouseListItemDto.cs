namespace Api.Responses;

/// <summary>
/// Describes a warehouse list item.
/// </summary>
public sealed record WarehouseListItemDto(
    long Id,
    string Name,
    bool Enabled,
    int UsersCount,
    int InboundsCount);
