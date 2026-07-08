using Xray.Config.Enums;

namespace Api.Responses;

/// <summary>
/// Describes an outbound assigned to a remote node.
/// </summary>
public sealed record NodeOutboundDto(
    int Id,
    string Tag,
    Protocol Protocol,
    StreamNetwork? Network,
    StreamSecurity? Security,
    bool Enabled,
    bool ReadOnly,
    string Config);
