namespace Api.Responses;

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpireAt,
    AdminDto Admin);
