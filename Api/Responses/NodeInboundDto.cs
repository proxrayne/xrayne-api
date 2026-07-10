using Xray.Config.Enums;

namespace Api.Responses;

/// <summary>
/// Describes an inbound assigned to a remote node.
/// </summary>
public sealed record NodeInboundDto(
    long Id,
    string Tag,
    string Port,
    Protocol Protocol,
    StreamNetwork? Network,
    StreamSecurity? Security,
    bool Enabled,
    bool ReadOnly,
    string Config);
