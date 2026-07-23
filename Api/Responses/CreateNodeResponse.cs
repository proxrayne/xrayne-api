namespace Api.Responses;

/// <summary>
/// Response returned after creating a remote node connection.
/// </summary>
public sealed record CreateNodeResponse(NodeDto Node);
