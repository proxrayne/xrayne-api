namespace Node.Models;

/// <summary>
/// Describes generated certificate and key material from a remote node.
/// </summary>
public sealed record GetCertsResponse(
    IReadOnlyList<string> Certificates,
    IReadOnlyList<string> Keys);
