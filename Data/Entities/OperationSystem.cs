using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

[Table("OperationSystems")]
public sealed class OperationSystemEntity : CreateUpdateEntity
{
    [Key]
    [MaxLength(32)]
    public required string Id { get; set; }

    [MaxLength(64)]
    public required string Name { get; set; }

    [MaxLength(512)]
    public string Note { get; set; } = string.Empty;

    [MaxLength(64)]
    public required string Icon { get; set; }

    public bool Enabled { get; set; } = true;

    public ImageEntity Image { get; set; } = null!;

    public ICollection<ApplicationEntity> Applications { get; set; } = [];
}
