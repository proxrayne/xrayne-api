using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Enums;

namespace Data.Entities;

/// <summary>
/// Stores a client application profile used by user connections.
/// </summary>
[Table("Applications")]
public sealed class ApplicationEntity : CreateUpdateEntity
{
    /// <summary>
    /// Gets or sets the application identifier.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the application display name.
    /// </summary>
    [MaxLength(64)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the optional application website URL.
    /// </summary>
    [MaxLength(64)]
    public string? WebsiteUrl { get; set; }

    /// <summary>
    /// Gets or sets the optional icon reference.
    /// </summary>
    [MaxLength(256)]
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the optional protocol label used for detection.
    /// </summary>
    [MaxLength(24)]
    public string? Protocol { get; set; }

    /// <summary>
    /// Gets or sets the pattern used to detect this application.
    /// </summary>
    [MaxLength(128)]
    public required string DetectPattern { get; set; }

    /// <summary>
    /// Gets or sets the subscription format produced for this application.
    /// </summary>
    public SubscriptionFormat SubscriptionFormat { get; set; }

    /// <summary>
    /// Gets or sets whether the application is available for use.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets application asset references.
    /// </summary>
    [Column(TypeName = "jsonb")]
    public List<string> Assets { get; set; } = [];

    /// <summary>
    /// Gets or sets connections that use this application profile.
    /// </summary>
    public List<ConnectionEntity> Connections { get; set; } = [];

    /// <summary>
    /// Gets or sets the administrator that owns the application profile.
    /// </summary>
    public AdminAccountEntity Admin { get; set; } = null!;
}
