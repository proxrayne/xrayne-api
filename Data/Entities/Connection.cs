using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xray.Config.Enums;

namespace Data.Entities;

[Table("Devices")]
public sealed class ConnectionEntity : CreateUpdateEntity
{
    [Key]
    public long Id { get; set; }

    [MaxLength(32)]
    public string? Name { get; set; }

    [MaxLength(64)]
    public string? HWID { get; set; }

    [MaxLength(64)]
    public string? OS { get; set; }

    [MaxLength(64)]
    public string? Model { get; set; }

    [MaxLength(64)]
    public string? AppVersion { get; set; }

    [MaxLength(64)]
    public required string Password { get; set; }

    public Guid Uuid { get; set; } = Guid.NewGuid();

    public XtlsFlow Flow { get; set; } = XtlsFlow.None;

    public EncryptionMethod Method { get; set; } = EncryptionMethod.None;

    // relation tables
    public UserEntity User { get; set; } = null!;

    public ApplicationEntity? Application { get; set; }
}