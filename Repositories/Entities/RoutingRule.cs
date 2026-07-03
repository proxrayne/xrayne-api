using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xray.Config.Models;

namespace Repositories.Entities;

[Table("RoutingRules")]
public class RoutingRuleEntity : CreateUpdateEntity
{
    [Key]
    public long Id { get; set; }

    [MaxLength(128)]
    public required string Tag { get; set; }

    public bool Enabled { get; set; } = true;

    public bool ReadOnly { get; set; } = false;

    public int Position { get; set; }

    [Column(TypeName = "jsonb")]
    public required RoutingRule Config { get; set; }

    // relation tables
    public AdminAccount Admin { get; set; } = null!;

    public NodeEntity Node { get; set; } = null!;
}