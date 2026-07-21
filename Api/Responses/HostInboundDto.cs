namespace Api.Responses;

/// <summary>
/// Describes an inbound option used by hosts.
/// </summary>
public sealed record HostInboundDto(
    long Id,
    string Tag,
    string Port,
    bool Enabled,
    string NodeName,
    string? ServerName);
