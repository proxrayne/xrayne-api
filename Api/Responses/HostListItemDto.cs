namespace Api.Responses;

/// <summary>
/// Describes a host row in the host management list.
/// </summary>
public sealed record HostListItemDto(
    long Id,
    string Name,
    string Address,
    string CountryAlpha2Code,
    HostInboundDto Inbound,
    bool Enabled,
    int Position);
