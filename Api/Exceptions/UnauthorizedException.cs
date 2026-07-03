namespace Api.Exceptions;

public sealed class UnauthorizedException : ApiException
{
    public UnauthorizedException(string detail) : base(StatusCodes.Status401Unauthorized, "Unauthorized", detail) { }
}
