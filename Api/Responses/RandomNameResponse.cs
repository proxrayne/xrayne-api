namespace Api.Responses;

/// <summary>
/// Response containing a generated random name.
/// </summary>
/// <param name="Name">Generated random name.</param>
public sealed record RandomNameResponse(string Name);
