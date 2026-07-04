namespace Api.Responses;

/// <summary>
/// Response returned when scheduling or applying a node operation.
/// </summary>
public sealed record NodeOperationResponse(NodeDto Node, string Status);
