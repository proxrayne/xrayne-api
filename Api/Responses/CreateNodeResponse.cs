namespace Api.Responses;

/// <summary>
/// Response returned after scheduling remote node creation.
/// </summary>
public sealed record CreateNodeResponse(NodeDto Node, string JobId);
