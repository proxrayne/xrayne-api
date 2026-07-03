namespace Api.Exceptions;

public sealed class BadRequestException : ApiException
{
    public BadRequestException(string detail)
        : base(StatusCodes.Status400BadRequest, "Bad Request", detail)
    {
    }
}
