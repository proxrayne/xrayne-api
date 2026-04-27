namespace XRayne.Api.Exceptions;

public sealed class ConflictException : ApiException
{
    public ConflictException(string detail)
        : base(StatusCodes.Status409Conflict, "Conflict", detail)
    {
    }
}
