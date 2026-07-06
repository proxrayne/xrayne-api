using Contracts.Enums;

namespace Api.Responses;

/// <summary>
/// Response model for saved remote node connection parameters.
/// </summary>
public sealed record NodeConnectionParametersResponse(
    string Address,
    int Port,
    int ApiPort,
    string SSHUsername,
    SSHAuthType AuthType,
    bool HasPassword,
    bool HasSSHKey,
    string WorkingDirectory);
