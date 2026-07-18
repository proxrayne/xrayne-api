using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

/// <summary>
/// Stores certificate metadata and remote node file paths.
/// </summary>
[Table("Certificates")]
public class CertificateEntity : CreateUpdateEntity
{
    /// <summary>
    /// Gets or sets the database identifier of the certificate.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the certificate domain name.
    /// </summary>
    [MaxLength(128)]
    public required string Domain { get; set; }

    /// <summary>
    /// Gets or sets the remote path to the certificate file.
    /// </summary>
    [MaxLength(2048)]
    public required string CertificateFile { get; set; }

    /// <summary>
    /// Gets or sets the remote path to the private key file.
    /// </summary>
    [MaxLength(2048)]
    public required string PrivateKeyFile { get; set; }

    /// <summary>
    /// Gets or sets whether the certificate is currently active.
    /// </summary>
    public bool Active { get; set; } = true;

    /// <summary>
    /// Gets or sets when the certificate expires.
    /// </summary>
    public DateTime ExpireAt { get; set; }

    /// <summary>
    /// Gets or sets the remote node that owns the certificate files.
    /// </summary>
    public NodeEntity Node { get; set; } = null!;

    /// <summary>
    /// Gets or sets the administrator that owns the certificate metadata.
    /// </summary>
    public AdminAccountEntity Admin { get; set; } = null!;
}
