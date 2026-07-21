namespace Api.Responses;

/// <summary>
/// Describes a subscription host.
/// </summary>
public sealed record HostDto(
    long Id,
    string Name,
    string Address,
    string CountryAlpha2Code,
    long InboundId,
    HostInboundDto Inbound,
    int? Port,
    string? ServerName,
    string? Host,
    string? Path,
    string Security,
    IReadOnlyList<string>? Alpn,
    string Fingerprint,
    string? FragmentTemplate,
    string? NoiseTemplate,
    bool Enabled,
    bool IsMuxEnabled,
    bool IsUseServerNameAsHost,
    bool IsRandomUseragent,
    bool AllowIncrease,
    int Position);
