using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XRayne.Repositories.Entities;

[Table("Certificates")]
public class CertificateEntity : CreateUpdateEntity
{
    [Key]
    public int Id { get; set; }

    [MaxLength(128)]
    public required string Domain { get; set; }

    [MaxLength(2048)]
    public required string CertificateFile { get; set; }

    [MaxLength(2048)]
    public required string PrivateKeyFile { get; set; }

    public bool Active { get; set; } = true;

    public DateTime ExpireAt { get; set; }

    public required string Certificate { get; set; }

    public required string PrivateKey { get; set; }

    // relation tables
    public NodeEntity Node { get; set; } = null!;

    public AdminAccount Admin { get; set; } = null!;
}