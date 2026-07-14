namespace Infrastructure.Dto;

/// <summary>
/// Describes one readonly routing rule enabled-state change in a batch save operation.
/// </summary>
public sealed record NodeRoutingRuleReadonlySaveItem(
    long Id,
    bool Enabled);
