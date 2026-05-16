using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xray.Config.Enums;
using Xray.Config.Models;

namespace XRayne.Repositories.Entities;

[Table("outbounds")]
public sealed class OutboundEntity : CreateUpdateEntity
{
    [Key]
    public int Id { get; set; }

    public int Index { get; set; }

    [Column(TypeName = "jsonb")]
    public required Outbound Native { get; set; }

    public AdminAccount Admin { get; set; } = null!;

    // Computed

    [NotMapped]
    public string? Tag => Native.Tag;

    [NotMapped]
    public Protocol Protocol => Native.Protocol;

    [NotMapped]
    public StreamNetwork Network => Native.StreamSettings!.Network;

    [NotMapped]
    public StreamSecurity Security => Native.StreamSettings!.Security;
}