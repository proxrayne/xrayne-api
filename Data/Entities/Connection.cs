using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Contracts.Enums;
using Contracts.Utilities;
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
    [MaxLength(64)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the optional User-Agent value reported by the client.
    /// </summary>
    [MaxLength(512)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the connection password.
    /// </summary>
    [MaxLength(64)]
    public string Password { get; set; } = XraySecretGenerator.GeneratePassword();

    /// <summary>
    /// Gets whether this connection currently has an observed client session.
    /// </summary>
    [NotMapped]
    public bool IsConnected => ConnectedAt != null && !string.IsNullOrEmpty(UserAgent);

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
    /// Gets or sets optional device information reported by the client.
    /// </summary>
    [Column(TypeName = "jsonb")]
    public DeviceInfo? DeviceInfo { get; set; }

    /// <summary>
    /// Gets or sets how the connection device should be verified.
    /// </summary>
    [Column(TypeName = "device_verification_method")]
    public DeviceVerificationMethod DeviceVerificationMethod { get; set; } = DeviceVerificationMethod.None;

    /// <summary>
    /// Gets or sets when this connection was last observed online.
    /// </summary>
    public DateTimeOffset? OnlineAt { get; set; }

    /// <summary>
    /// Gets or sets when this connection last established a client session.
    /// </summary>
    public DateTimeOffset? ConnectedAt { get; set; }

    /// <summary>
    /// Gets or sets when this connection last refreshed its subscription.
    /// </summary>
    public DateTimeOffset? SubscriptionUpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets when this connection was revoked.
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    /// Gets or sets whether this connection has been revoked.
    /// </summary>
    public bool Revoked { get; set; }

    /// <summary>
    /// Gets or sets the user identifier that owns this connection.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Gets or sets the user that owns this connection.
    /// </summary>
    public UserEntity User { get; set; } = null!;

    /// <summary>
    /// Gets or sets the optional operating system identifier reported by the client.
    /// </summary>
    [MaxLength(32)]
    public string? OperationSystemId { get; set; }

    /// <summary>
    /// Gets or sets the optional operating system reported by the client.
    /// </summary>
    public OperationSystemEntity? OperatingSystem { get; set; }

    /// <summary>
    /// Gets or sets the optional application profile identifier.
    /// </summary>
    public int? ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the optional application profile associated with this connection.
    /// </summary>
    public ApplicationEntity? Application { get; set; }
}
