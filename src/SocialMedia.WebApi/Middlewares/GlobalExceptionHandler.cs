
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SocialMedia.WebApi.Middlewares;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "server.error",
            Detail = "An unexpected error occurred.",
            Instance = context.Request.Path
        };

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}