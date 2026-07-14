namespace Infrastructure.Dto;

/// <summary>
/// Describes one manually managed routing rule in a batch save operation.
/// </summary>
public sealed record NodeRoutingRuleManualSaveItem(
    long? Id,
    string Config,
    bool Enabled);
