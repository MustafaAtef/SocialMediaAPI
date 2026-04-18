using Microsoft.AspNetCore.Mvc;

using SocialMedia.Core.Errors;

namespace SocialMedia.Api.Controllers;

[ApiController]
public abstract class ApiController : ControllerBase
{
    [NonAction]
    protected ActionResult HandleFailure(Core.Abstractions.IResult result)
    {
        var firstError = result.Error;

        return firstError.Type switch
        {
            ErrorType.NotFound => NotFound(ToProblemDetails(result)),
            ErrorType.Validation => BadRequest(ToValidationProblemDetails(result)),
            ErrorType.Conflict => Conflict(ToProblemDetails(result)),
            ErrorType.Unauthorized => Unauthorized(ToProblemDetails(result)),
            ErrorType.Forbidden => Forbid(),
            _ => StatusCode(500, ToProblemDetails(result))
        };
    }

    private static ProblemDetails ToProblemDetails(Core.Abstractions.IResult result) => new()
    {
        Title = result.Error.Code,
        Detail = result.Error.Message,
        Status = GetStatusCode(result.Error.Type)
    };

    // Formats all validation errors as a proper ValidationProblemDetails
    private static ValidationProblemDetails ToValidationProblemDetails(Core.Abstractions.IResult result)
    {
        var details = new ValidationProblemDetails
        {
            Title = "validation.failed",
            Detail = "One or more validation errors occurred.",
            Status = 400
        };

        foreach (var error in result.Errors)
        {
            if (details.Errors.ContainsKey(error.Code))
                details.Errors[error.Code] = [.. details.Errors[error.Code], error.Message];
            else
                details.Errors[error.Code] = [error.Message];
        }

        return details;
    }

    private static int GetStatusCode(ErrorType type) => type switch
    {
        ErrorType.NotFound => 404,
        ErrorType.Validation => 400,
        ErrorType.Conflict => 409,
        ErrorType.Unauthorized => 401,
        ErrorType.Forbidden => 403,
        _ => 500
    };
}