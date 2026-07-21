using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Values;
using Microsoft.EntityFrameworkCore;

namespace Data.Entities;

/// <summary>
/// Stores image content used by panel-managed entities.
/// </summary>
[Table("Images")]
[Index(nameof(Key), IsUnique = true)]
public sealed class ImageEntity : CreateUpdateEntity
{
    /// <summary>
    /// Gets or sets the image identifier.
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the public image key.
    /// </summary>
    [MaxLength(128)]
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets optional alternate text for the image.
    /// </summary>
    [MaxLength(64)]
    public string? Alt { get; set; }

    /// <summary>
    /// Gets or sets the binary image payload.
    /// </summary>
    [Column(TypeName = "bytea")]
    public required byte[] Content { get; set; }

    /// <summary>
    /// Gets or sets the image MIME type.
    /// </summary>
    [MaxLength(32)]
    public required string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the image version used for browser cache invalidation.
    /// </summary>
    public long Version { get; set; } = 1;

    /// <summary>
    /// Gets the public versioned image URL.
    /// </summary>
    [NotMapped]
    public string Url => ImageUrl.Build(Key, Version);
}
