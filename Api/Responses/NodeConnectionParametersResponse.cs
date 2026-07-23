namespace Api.Responses;

/// <summary>
/// Response model for saved remote node connection parameters.
/// </summary>
public sealed record NodeConnectionParametersResponse(
    string Address,
    int ApiPort,
    string ApiKeyFingerprint);
