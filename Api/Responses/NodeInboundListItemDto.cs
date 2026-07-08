using Xray.Config.Enums;

namespace Api.Responses;

/// <summary>
/// Describes an inbound assigned to a remote node without its full JSON configuration.
/// </summary>
public sealed record NodeInboundListItemDto(
    int Id,
    string Tag,
    string Port,
    Protocol Protocol,
    StreamNetwork? Network,
    StreamSecurity? Security,
    bool Enabled,
    bool ReadOnly);
