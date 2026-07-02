using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;

namespace XRayne.Infrastructure.Services;

/// <summary>
/// Uses ASP.NET Core data protection to store remote node API keys encrypted at rest.
/// </summary>
public sealed class NodeSecretService(IDataProtectionProvider dataProtectionProvider) : INodeSecretService
{
    private readonly IDataProtector protector = dataProtectionProvider.CreateProtector("xrayne.remote-node-api-key.v1");

    public string GenerateApiKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        return WebEncoders.Base64UrlEncode(bytes);
    }

    public string ProtectApiKey(string apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        return protector.Protect(apiKey);
    }

    public string UnprotectApiKey(string encryptedApiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedApiKey);

        return protector.Unprotect(encryptedApiKey);
    }

    public string GetFingerprint(string apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(apiKey));

        return Convert.ToHexString(hash[..8]).ToLowerInvariant();
    }
}
