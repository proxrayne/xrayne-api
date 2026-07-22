using Contracts.Enums;
using OptionalValues;
using OptionalValues.DataAnnotations;
using Xray.Config.Enums;

namespace Api.Requests;

/// <summary>
/// Defines optional fields used to patch a user connection.
/// </summary>
public sealed class PatchConnectionRequest
{
    /// <summary>
    /// Gets optional connection display name.
    /// </summary>
    [OptionalMaxLength(64)]
    public OptionalValue<string> Name { get; init; }

    /// <summary>
    /// Gets optional XTLS flow.
    /// </summary>
    public OptionalValue<XtlsFlow> Flow { get; init; }

    /// <summary>
    /// Gets optional encryption method.
    /// </summary>
    public OptionalValue<EncryptionMethod> Method { get; init; }

    /// <summary>
    /// Gets optional device verification method.
    /// </summary>
    public OptionalValue<DeviceVerificationMethod> DeviceVerificationMethod { get; init; }
}
