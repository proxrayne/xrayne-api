using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

/// <summary>
/// Stores an operating system option that can be linked to applications.
/// </summary>
[Table("OperationSystems")]
public sealed class OperationSystemEntity : CreateUpdateEntity
{
    /// <summary>
    /// Gets or sets the stable operating system identifier.
    /// </summary>
    [Key]
    [MaxLength(32)]
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the operating system display name.
    /// </summary>
    [MaxLength(64)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets an optional note.
    /// </summary>
    [MaxLength(512)]
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the operating system is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the linked image identifier.
    /// </summary>
    public long ImageId { get; set; }

    /// <summary>
    /// Gets or sets the linked image.
    /// </summary>
    public ImageEntity Image { get; set; } = null!;

    /// <summary>
    /// Gets or sets applications linked to this operating system.
    /// </summary>
    public ICollection<ApplicationEntity> Applications { get; set; } = [];
}
