namespace RemoteNode.Models;

/// <summary>
/// Describes remote node network addresses.
/// </summary>
public sealed record NodeNetworkStats(
    IReadOnlyCollection<string> IPv4Addresses,
    IReadOnlyCollection<string> IPv6Addresses);
