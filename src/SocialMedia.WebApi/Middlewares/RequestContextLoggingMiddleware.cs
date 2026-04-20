
using Serilog.Context;

namespace SocialMedia.WebApi.Middlewares;

public class RequestContextLoggingMiddleware
{
    private readonly string _correlationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public RequestContextLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
    private string GetOrCreateCorrelationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue(_correlationIdHeader, out var correlationId);
        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }
}

public static class RequestContextLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestContextLoggingMiddleware>();
    }
}
