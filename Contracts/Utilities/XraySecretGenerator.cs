using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace Contracts.Utilities;

/// <summary>
/// Generates xray-core compatible random secrets.
/// </summary>
public static class XraySecretGenerator
{
    private const int DefaultPasswordByteCount = 32;
    private const int DefaultSeqByteCount = 16;

    /// <summary>
    /// Generates a Shadowsocks password.
    /// </summary>
    public static string GeneratePassword()
    {
        return GenerateShadowsocksPassword();
    }

    /// <summary>
    /// Generates a Shadowsocks password.
    /// </summary>
    public static string GenerateShadowsocksPassword()
    {
        return GenerateBase64UrlSecret(DefaultPasswordByteCount);
    }

    /// <summary>
    /// Generates a Hysteria password.
    /// </summary>
    public static string GenerateHysteriaPassword()
    {
        return GenerateBase64UrlSecret(DefaultPasswordByteCount);
    }

    /// <summary>
    /// Generates a sequence secret.
    /// </summary>
    public static string GenerateSeq()
    {
        return GenerateBase64UrlSecret(DefaultSeqByteCount);
    }

    /// <summary>
    /// Generates a Base64Url encoded random secret with the requested byte count.
    /// </summary>
    public static string GenerateBase64UrlSecret(int byteCount)
    {
        if (byteCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteCount), "Secret byte count must be greater than zero.");
        }

        var bytes = RandomNumberGenerator.GetBytes(byteCount);

        return WebEncoders.Base64UrlEncode(bytes);
    }
}
