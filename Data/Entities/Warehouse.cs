using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

/// <summary>
/// Groups users and inbounds for subscription generation.
/// </summary>
[Table("Warehouses")]
public sealed class WarehouseEntity : CreateUpdateEntity
{
    /// <summary>
    /// Gets or sets the warehouse identifier.
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the warehouse display name.
    /// </summary>
    [MaxLength(64)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the optional warehouse note.
    /// </summary>
    [MaxLength(512)]
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the warehouse is available for subscriptions.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets inbounds included in this warehouse.
    /// </summary>
    public List<InboundEntity> Inbounds { get; set; } = [];

    /// <summary>
    /// Gets or sets users assigned to this warehouse.
    /// </summary>
    public List<UserEntity> Users { get; set; } = [];

    /// <summary>
    /// Gets or sets the administrator that owns the warehouse.
    /// </summary>
    public AdminAccountEntity Admin { get; set; } = null!;
}
