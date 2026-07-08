namespace Api.Responses;

/// <summary>
/// Describes a routing rule assigned to a remote node.
/// </summary>
public sealed record NodeRoutingRuleDto(
    long Id,
    string Tag,
    IReadOnlyCollection<string> InboundTags,
    string? OutboundTag,
    bool Enabled,
    bool ReadOnly,
    int Position,
    string Config);
