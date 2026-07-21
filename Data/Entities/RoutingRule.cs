using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xray.Config.Models;

namespace Data.Entities;

[Table("RoutingRules")]
public class RoutingRuleEntity : CreateUpdateEntity
{
    [Key]
    public long Id { get; set; }

    public bool Enabled { get; set; } = true;

    public bool ReadOnly { get; set; } = false;

    public int Position { get; set; }

    [Column(TypeName = "jsonb")]
    public required RoutingRule Config { get; set; }

    // relation tables
    public long AdminId { get; set; }

    public AdminAccountEntity Admin { get; set; } = null!;

    public long NodeId { get; set; }

    public NodeEntity Node { get; set; } = null!;

    // Computed

    [NotMapped]
    public ICollection<string> InboundTags => Config.InboundTag ?? [];

    [NotMapped]
    public string? OutboundTag => Config.OutboundTag;

    [NotMapped]
    public string? RuleTag => Config.RuleTag;
}
