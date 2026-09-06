using CPMS.API.Exceptions;
using CPMS.BuildingBlocks.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CPMS.API.Infrastructure;

/// <summary>
/// Maps exceptions thrown by handlers to HTTP responses, so controllers do not need try/catch blocks.
/// </summary>
public class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            BusinessRuleValidationException => (StatusCodes.Status400BadRequest, "Business rule violated"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument"),
            _ => (StatusCodes.Status500InternalServerError, "Internal server error")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        else
            _logger.LogWarning("{Method} {Path} -> {Status}: {Message}", httpContext.Request.Method, httpContext.Request.Path, status, exception.Message);

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError ? null : exception.Message,
            Instance = httpContext.Request.Path
        }, cancellationToken);

        return true;
    }
}
