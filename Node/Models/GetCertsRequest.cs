namespace Node.Models;

/// <summary>
/// Requests self-signed certificate generation from a remote node.
/// </summary>
public sealed record GetCertsRequest(
    IReadOnlyCollection<string> Domains,
    string? CommonName,
    string? Organization,
    bool IsCA,
    string? Expire);
