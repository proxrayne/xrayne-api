namespace Api.Responses;

/// <summary>
/// Remote node certificate metadata response.
/// </summary>
public sealed record NodeCertificateDto(
    int Id,
    string Domain,
    bool Active,
    DateTime ExpireAt,
    string CertificateFile,
    string PrivateKeyFile,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
