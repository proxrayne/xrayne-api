namespace Api.Requests;

/// <summary>
/// Request to import a certificate and private key from remote node file paths.
/// </summary>
public sealed record UploadNodeCertificateRequest(
    string Domain,
    string CertificateFile,
    string PrivateKeyFile);
