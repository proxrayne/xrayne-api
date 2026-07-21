using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Data.Entities;

/// <summary>
/// Stores panel metadata for a geo resource file assigned to a remote node.
/// </summary>
[Table("GeoResources")]
[Index(nameof(NextRunAt))]
[Index(nameof(NodeId), nameof(Filename), IsUnique = true)]
public class GeoResourceEntity : CreateUpdateEntity
{
    /// <summary>
    /// Gets or sets the geo resource identifier.
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the file name stored on the remote node.
    /// </summary>
    [MaxLength(128)]
    public required string Filename { get; set; }

    /// <summary>
    /// Gets or sets the stored file size in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the last write time reported by the remote node.
    /// </summary>
    public DateTimeOffset LastModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the latest processing status for upload or update work.
    /// </summary>
    [Column(TypeName = "geo_resource_status")]
    public GeoResourceStatus Status { get; set; } = GeoResourceStatus.Success;

    /// <summary>
    /// Gets or sets the transient processing message buffer.
    /// </summary>
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Gets or sets the URL used by auto-updated geo resources.
    /// </summary>
    [MaxLength(2048)]
    public string? Url { get; set; }

    /// <summary>
    /// Gets whether this geo resource is refreshed from a URL.
    /// </summary>
    [NotMapped]
    public bool IsAutoUpdate => Url is not null;

    /// <summary>
    /// Gets or sets the Unix cron template used by auto-updated geo resources.
    /// </summary>
    public int? UpdateInterval { get; set; }

    /// <summary>
    /// Gets or sets when the auto-updated geo resource should refresh next.
    /// </summary>
    public DateTimeOffset? NextRunAt { get; set; }

    /// <summary>
    /// Gets or sets when the last auto-update failure happened.
    /// </summary>
    public DateTimeOffset? LastErrorAt { get; set; }

    /// <summary>
    /// Gets or sets the remote node identifier that owns the geo resource.
    /// </summary>
    public long NodeId { get; set; }

    /// <summary>
    /// Gets or sets the remote node that owns the geo resource.
    /// </summary>
    public NodeEntity Node { get; set; } = null!;

    /// <summary>
    /// Gets or sets the administrator identifier that owns the geo resource.
    /// </summary>
    public long AdminId { get; set; }

    /// <summary>
    /// Gets or sets the administrator that owns the geo resource.
    /// </summary>
    public AdminAccountEntity Admin { get; set; } = null!;
}
