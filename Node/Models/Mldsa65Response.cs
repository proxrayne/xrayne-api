namespace Node.Models;

/// <summary>
/// Describes generated ML-DSA-65 key material from a remote node.
/// </summary>
public sealed record Mldsa65Response(
    string Seed,
    string Verify);
