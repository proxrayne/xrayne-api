using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xray.Config.Enums;

namespace Data.Entities;

/// <summary>
/// Stores one user connection credential set.
/// </summary>
[Table("Connections")]
public sealed class ConnectionEntity : CreateUpdateEntity
{
    /// <summary>
    /// Gets or sets the connection identifier.
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the optional connection display name.
    /// </summary>
    [MaxLength(32)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the optional hardware identifier reported by the client.
    /// </summary>
    [MaxLength(64)]
    public string? HWID { get; set; }

    /// <summary>
    /// Gets or sets the optional operating system name reported by the client.
    /// </summary>
    [MaxLength(64)]
    public string? OS { get; set; }

    /// <summary>
    /// Gets or sets the optional device model reported by the client.
    /// </summary>
    [MaxLength(64)]
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets the optional application version reported by the client.
    /// </summary>
    [MaxLength(64)]
    public string? AppVersion { get; set; }

    /// <summary>
    /// Gets or sets the connection password.
    /// </summary>
    [MaxLength(64)]
    public required string Password { get; set; }

    /// <summary>
    /// Gets or sets the connection UUID.
    /// </summary>
    public Guid Uuid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the XTLS flow for this connection.
    /// </summary>
    [Column(TypeName = "xtls_flow")]
    public XtlsFlow Flow { get; set; } = XtlsFlow.None;

    /// <summary>
    /// Gets or sets the encryption method for this connection.
    /// </summary>
    [Column(TypeName = "encryption_method")]
    public EncryptionMethod Method { get; set; } = EncryptionMethod.None;

    /// <summary>
    /// Gets or sets the user identifier that owns this connection.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Gets or sets the user that owns this connection.
    /// </summary>
    public UserEntity User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the optional application profile identifier.
    /// </summary>
    public int? ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the optional application profile associated with this connection.
    /// </summary>
    public ApplicationEntity? Application { get; set; }
}
