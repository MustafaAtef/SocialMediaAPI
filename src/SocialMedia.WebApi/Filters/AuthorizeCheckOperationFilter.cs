using System;

using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;
namespace SocialMedia.WebApi.Filters;

public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if the endpoint has Authorize attribute and no AllowAnonymous attribute 
        var hasAuthorize = (context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ?? false) || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any(); var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any(); if (hasAuthorize && !hasAllowAnonymous)
        {
            // Add security requirement for Bearer token
            operation.Security = new List<OpenApiSecurityRequirement> { new OpenApiSecurityRequirement { { new OpenApiSecuritySchemeReference("Bearer", null, null), new List<string>() } } };
        }
        else
        {
            // Ensure no security requirement for non-authorized endpoints
            operation.Security = new List<OpenApiSecurityRequirement>();
        }
    }
}