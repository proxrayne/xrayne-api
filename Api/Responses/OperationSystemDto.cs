namespace Api.Responses;

/// <summary>
/// Describes an operating system option.
/// </summary>
public sealed record OperationSystemDto(
    string Id,
    string Name,
    string Note,
    bool Enabled,
    ImageDto Image);
