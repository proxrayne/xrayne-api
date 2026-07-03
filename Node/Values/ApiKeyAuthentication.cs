namespace Node.Values;

/// <summary>
/// Defines API-key authentication constants for the remote node API.
/// </summary>
public static class ApiKeyAuthentication
{
    /// <summary>
    /// HTTP header used to pass the remote node API key.
    /// </summary>
    public const string HeaderName = "X-Node-Api-Key";

    /// <summary>
    /// OpenAPI security scheme name for API-key authentication.
    /// </summary>
    public const string ApiKeySchemeName = "ApiKey";
}
