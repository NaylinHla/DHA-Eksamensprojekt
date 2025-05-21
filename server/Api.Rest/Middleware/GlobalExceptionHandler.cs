using System.ComponentModel.DataAnnotations;
using System.Security.Authentication;
using Core.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Middleware;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var exceptionMessage = exception.Message;
        logger.LogInformation("Exception: {ExceptionType} with this message: {ExceptionMessage}",
            exception.GetType().Name, exceptionMessage);

        var status = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
            AuthenticationException => StatusCodes.Status401Unauthorized,
            UnauthorizedAccessException => StatusCodes.Status403Forbidden,
            FileNotFoundException => StatusCodes.Status404NotFound,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            NotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };


        // Problem details structure
        var problemDetails = new ProblemDetails
        {
            Title = exception.Message,
            Detail = exception.InnerException?.Message,
            Status = status
        };

        httpContext.Response.StatusCode = status;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}