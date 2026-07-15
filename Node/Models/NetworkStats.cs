namespace Node.Models;

/// <summary>
/// Describes remote node network addresses.
/// </summary>
public sealed record NetworkStats(
    IReadOnlyCollection<string> IPv4Addresses,
    IReadOnlyCollection<string> IPv6Addresses);
