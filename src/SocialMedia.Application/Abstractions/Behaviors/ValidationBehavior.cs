using FluentValidation;

using MediatR;

using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Abstractions.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResult
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errors = failures
            .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))
            .ToList();

        return CreateValidationResult(errors);
    }

    private static TResponse CreateValidationResult(List<Error> errors)
    {
        // If TResponse is exactly Result (non-generic, no TValue)
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(IResult)Result.ValidationFailure(errors);

        // TResponse is Result<TValue> — extract TValue and call the generic overload
        var valueType = typeof(TResponse).GetGenericArguments()[0];

        var method = typeof(Result)
            .GetMethod(
                nameof(Result.ValidationFailure),
                1,
                [typeof(IReadOnlyList<Error>)])!
            .MakeGenericMethod(valueType);

        return (TResponse)method.Invoke(null, [errors])!;
    }
}