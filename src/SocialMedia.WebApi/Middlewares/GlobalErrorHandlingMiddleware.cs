using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using SocialMedia.Core.Exceptions;

namespace SocialMedia.WebApi.Middlewares;

public class GlobalErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnAuthenticatedException unauthenticatedEx)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails()
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = "The user not authenticated",
                Detail = unauthenticatedEx.Message,
            });
        }
        catch (UnAuthorizedException unauthorizedEx)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails()
            {
                Status = (int)HttpStatusCode.Forbidden,
                Title = "The user not authorized",
                Detail = unauthorizedEx.Message,
            });
        }
        catch (BadRequestException badRequestEx)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails()
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Bad Request",
                Detail = badRequestEx.Message,
            });
        }
        catch (UniqueException uniqueEx)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails()
            {
                Status = (int)HttpStatusCode.Conflict,
                Title = "Conflict Error",
                Detail = uniqueEx.Message,
            });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ProblemDetails()
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Internal Server Error",
                Detail = ex.Message,
            });
        }

    }
}

public static class GlobalErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalErrorHandlingMiddleware>();
    }
}