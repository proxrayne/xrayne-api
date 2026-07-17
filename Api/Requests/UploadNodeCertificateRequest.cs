using System.ComponentModel.DataAnnotations;

namespace Api.Requests;

/// <summary>
/// Request to import a certificate and private key from remote node file paths.
/// </summary>
public sealed class UploadNodeCertificateRequest
{
    [Required]
    [MaxLength(128)]
    public required string Domain { get; set; }

    [Required]
    [MaxLength(2048)]
    public required string CertificateFile { get; set; }

    [Required]
    [MaxLength(2048)]
    public required string PrivateKeyFile { get; set; }
}