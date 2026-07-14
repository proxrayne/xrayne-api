namespace Api.Responses;

/// <summary>
/// Describes a routing rule assigned to a remote node without its full JSON configuration.
/// </summary>
public sealed record NodeRoutingRuleListItemDto(
    long Id,
    string Tag,
    IReadOnlyCollection<string> InboundTags,
    string? OutboundTag,
    bool Enabled,
    bool ReadOnly,
    int Position,
    string Config);
