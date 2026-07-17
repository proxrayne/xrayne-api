using Api.Responses;
using Contracts.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters;

/// <summary>
/// Converts exceptions thrown by API actions into consistent error responses.
/// </summary>
public sealed class ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) : IExceptionFilter
{
    /// <inheritdoc />
    public int Order => int.MaxValue - 10;

    /// <inheritdoc />
    public void OnActionExecuting(ActionExecutingContext context) { }

    /// <inheritdoc />
    public void OnException(ExceptionContext context)
    {
        var response = context.Exception switch
        {
            ApiException ext => new ApiErrorResponse(ext.StatusCode, ext.Name, ext.Detail),
            ArgumentException ext => new ApiErrorResponse(StatusCodes.Status400BadRequest, "Bad Request", ext.Message),
            _ => new ApiErrorResponse(
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred.")
        };

        if (response.Status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(context.Exception, "API exception {StatusCode} {Title}", response.Status, response.Name);
        }
        else
        {
            logger.LogWarning(
                context.Exception,
                "API exception {StatusCode} {Title}: {Detail}",
                response.Status,
                response.Name,
                response.Detail);
        }

        context.Result = new ObjectResult(response) { StatusCode = response.Status };

        context.ExceptionHandled = true;
    }
}
