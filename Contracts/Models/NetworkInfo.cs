namespace Contracts.Models;

/// <summary>
/// Contains local server network addresses.
/// </summary>
public sealed record NetworkInfo(
    IReadOnlyCollection<string> IPv4Addresses,
    IReadOnlyCollection<string> IPv6Addresses);
