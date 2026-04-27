using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using XRayne.Api.Exceptions;
using XRayne.Api.Responses;

namespace XRayne.Api.Filters;

public sealed class ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) : IExceptionFilter
{
    public int Order => int.MaxValue - 10;

    public void OnException(ExceptionContext context)
    {
        var response = context.Exception switch
        {
            ApiException ext => new ApiErrorResponse(ext.StatusCode, ext.Name, ext.Detail),
            _ => new ApiErrorResponse(400, "BadRequest", context.Exception.Message)
        };

        logger.LogWarning(context.Exception, "API exception {StatusCode} {Title}: {Detail}",
           response.Status,
           response.Name,
        response.Detail
        );

        context.Result = new ObjectResult(response) { StatusCode = response.Status };

        context.ExceptionHandled = true;
    }
}
