using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

[Table("Images")]
public sealed class ImageEntity : CreateUpdateEntity
{
    [Key]
    public long Id { get; set; }

    [MaxLength(64)]
    public string? Alt { get; set; }

    public required string Content { get; set; }
}
