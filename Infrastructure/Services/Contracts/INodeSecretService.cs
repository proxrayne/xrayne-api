namespace Infrastructure.Services;

/// <summary>
/// Generates, protects, and identifies remote node API keys.
/// </summary>
public interface INodeSecretService
{
    string GenerateApiKey();

    string ProtectApiKey(string apiKey);

    string UnprotectApiKey(string encryptedApiKey);

    string GetFingerprint(string apiKey);
}
