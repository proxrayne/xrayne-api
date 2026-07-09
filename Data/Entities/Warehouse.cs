using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

[Table("Warehouses")]
public sealed class WarehouseEntity : CreateUpdateEntity
{
    [Key]
    public long Id { get; set; }

    [MaxLength(64)]
    public required string Name { get; set; }

    [MaxLength(512)]
    public string Note { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    // relation tables
    public List<InboundEntity> Inbounds { get; set; } = new();

    public List<UserEntity> Users { get; set; } = new();

    public AdminAccount Admin { get; set; } = null!;
}