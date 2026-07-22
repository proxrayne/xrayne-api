using Contracts.Enums;
using OptionalValues;
using Xray.Config.Enums;

namespace Data.Models;

/// <summary>
/// Describes optional fields used to update a user connection.
/// </summary>
public sealed class ConnectionPatch
{
    /// <summary>
    /// Gets optional connection display name.
    /// </summary>
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
