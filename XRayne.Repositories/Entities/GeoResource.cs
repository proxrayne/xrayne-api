using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace XRayne.Repositories.Entities;

[Table("geo_resources")]
[Index(nameof(NextRunAt))]
public class GeoResourceEntity : CreateUpdateEntity
{
    [Key]
    public long Id { get; set; }

    [MaxLength(128)]
    public required string Filename { get; set; }

    [MaxLength(2048)]
    public string? Url { get; set; }

    [MaxLength(32)]
    public string? CronTemplate { get; set; }

    public DateTime NextRunAt { get; set; }

    public DateTime? LastErrorAt { get; set; }

    public string? LastError { get; set; }

    // relation tables
    public NodeEntity Node { get; set; } = null!;

    public AdminAccount Admin { get; set; } = null!;
}