namespace Api.Exceptions;

/// <summary>
/// Represents an intended API error caused by an invalid client request.
/// </summary>
public sealed class BadRequestException : ApiException
{
    /// <summary>
    /// Creates a bad request API exception with a client-facing detail message.
    /// </summary>
    public BadRequestException(string detail)
        : base(StatusCodes.Status400BadRequest, "Bad Request", detail)
    {
    }
}
