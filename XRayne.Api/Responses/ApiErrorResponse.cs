namespace XRayne.Api.Responses;

public sealed record ApiErrorResponse(int Status, string Name, string Detail);
