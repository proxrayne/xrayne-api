using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xray.Config.Enums;
using Xray.Config.Models;

namespace XRayne.Repositories.Entities;

[Table("inbounds")]
public sealed class InboundEntity : CreateUpdateEntity
{
    [Key]
    public int Id { get; set; }

    public bool Enabled { get; set; }

    [MaxLength(256)]
    public required string DisplayName { get; set; }

    public DateTimeOffset? LastTrafficReset { get; set; }

    [Column(TypeName = "jsonb")]
    public required Inbound Native { get; set; }

    public List<User> Users { get; set; } = new();

    public AdminAccount Admin { get; set; } = null!;

    // Computed

    [NotMapped]
    public string Tag => Native.Tag;

    [NotMapped]
    public string? Listen => Native.Listen;

    [NotMapped]
    public Protocol Protocol => Native.Protocol;

    [NotMapped]
    public StreamNetwork Network => Native.StreamSettings.Network;

    [NotMapped]
    public StreamSecurity Security => Native.StreamSettings.Security;

    [NotMapped]
    public InboundSniffing? Sniffing => Native.Sniffing;

    [NotMapped]
    public Port Port => Native.Port;
}