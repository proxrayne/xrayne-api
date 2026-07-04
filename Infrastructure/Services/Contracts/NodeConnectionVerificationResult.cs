namespace Infrastructure.Services;

/// <summary>
/// Describes a successful node API verification result.
/// </summary>
public sealed record NodeConnectionVerificationResult(string? XrayVersion, DateTimeOffset VerifiedAt);
