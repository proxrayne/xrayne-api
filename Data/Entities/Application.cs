using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Enums;

namespace Data.Entities;

[Table("Applications")]
public sealed class ApplicationEntity : CreateUpdateEntity
{
    [Key]
    public int Id { get; set; }

    [MaxLength(64)]
    public required string Name { get; set; }

    [MaxLength(64)]
    public string? WebsiteUrl { get; set; }

    [MaxLength(256)]
    public string? Icon { get; set; }

    [MaxLength(24)]
    public string? Protocol { get; set; }

    [MaxLength(128)]
    public required string DetectPattern { get; set; }

    public SubscriptionFormat SubscriptionFormat { get; set; }

    public bool Enabled { get; set; } = true;

    public List<string> Assets { get; set; } = new();

    // relation tables
    public List<ConnectionEntity> Connections { get; set; } = new();

    public AdminAccount Admin { get; set; } = null!;
}