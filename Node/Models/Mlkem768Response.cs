namespace Node.Models;

/// <summary>
/// Describes generated ML-KEM-768 key material from a remote node.
/// </summary>
public sealed record Mlkem768Response(
    string Seed,
    string Client,
    string Hash);
