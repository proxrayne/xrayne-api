using Xray.Config.Enums;

namespace Api.Responses;

/// <summary>
/// Describes an outbound assigned to a remote node without its full JSON configuration.
/// </summary>
public sealed record NodeOutboundListItemDto(
    long Id,
    string Tag,
    Protocol Protocol,
    StreamNetwork? Network,
    StreamSecurity? Security,
    bool Enabled,
    bool ReadOnly);
