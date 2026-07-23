namespace Infrastructure.Services;

/// <summary>
/// Protects and identifies remote node API keys.
/// </summary>
public interface INodeSecretService
{
    string ProtectApiKey(string apiKey);

    string UnprotectApiKey(string encryptedApiKey);

    string GetFingerprint(string apiKey);
}
