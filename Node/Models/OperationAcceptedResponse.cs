namespace Node.Models;

/// <summary>
/// Describes an accepted remote node operation.
/// </summary>
public sealed record OperationAcceptedResponse(
    string Operation,
    string Status);
