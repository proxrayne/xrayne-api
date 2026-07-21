using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xray.Config.Enums;
using Xray.Config.Models;

namespace Data.Entities;

[Table("Outbounds")]
public sealed class OutboundEntity : CreateUpdateEntity
{
    [Key]
    public long Id { get; set; }

    public bool Enabled { get; set; }

    public bool ReadOnly { get; set; }

    [Column(TypeName = "jsonb")]
    public required Outbound Config { get; set; }

    public long AdminId { get; set; }

    public AdminAccountEntity Admin { get; set; } = null!;

    public long NodeId { get; set; }

    public NodeEntity Node { get; set; } = null!;

    // Computed

    [NotMapped]
    public string? Tag => Config.Tag;

    [NotMapped]
    public Protocol Protocol => Config.Protocol;

    [NotMapped]
    public StreamNetwork? Network => Config.StreamSettings?.Network;

    [NotMapped]
    public StreamSecurity? Security => Config.StreamSettings?.Security;
}
