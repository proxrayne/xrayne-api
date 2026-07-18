using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xray.Config.Enums;
using Xray.Config.Models;

namespace Data.Entities;

[Table("Inbounds")]
public sealed class InboundEntity : CreateUpdateEntity
{
    [Key]
    public long Id { get; set; }

    public bool Enabled { get; set; }

    public bool ReadOnly { get; set; }

    public DateTimeOffset? LastTrafficReset { get; set; }

    [Column(TypeName = "jsonb")]
    public required Inbound Config { get; set; }

    public AdminAccountEntity Admin { get; set; } = null!;

    public NodeEntity Node { get; set; } = null!;

    // Computed

    [NotMapped]
    public string Tag => Config.Tag;

    [NotMapped]
    public string? Listen => Config.Listen;

    [NotMapped]
    public Protocol Protocol => Config.Protocol;

    [NotMapped]
    public StreamNetwork? Network => Config.StreamSettings?.Network;

    [NotMapped]
    public StreamSecurity? Security => Config.StreamSettings?.Security;

    [NotMapped]
    public InboundSniffing? Sniffing => Config.Sniffing;

    [NotMapped]
    public Port Port => Config.Port;
}
