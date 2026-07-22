using System.ComponentModel.DataAnnotations;
using Contracts.Enums;
using Xray.Config.Enums;

namespace Api.Requests;

/// <summary>
/// Defines fields required to create a user connection.
/// </summary>
public sealed class CreateConnectionRequest
{
    public long UserId { get; set; }

    [Required]
    [MaxLength(64)]
    public required string Name { get; set; }

    public XtlsFlow Flow { get; set; }

    public EncryptionMethod Method { get; set; }

    public DeviceVerificationMethod DeviceVerificationMethod { get; set; }
}
