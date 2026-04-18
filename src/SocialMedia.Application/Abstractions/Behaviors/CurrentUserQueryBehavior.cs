
using MediatR;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;


namespace SocialMedia.Application.Abstractions.Behaviors;

public class CurrentUserQueryBehavior<TRequest, TResponse>(IUserService userService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICurrentUserQuery
{

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var user = userService.GetAuthenticatedUser();
        if (user is null)
        {
            var failure = Result.Failure<TResponse>(UserErrors.Unauthenticated);
            return (TResponse)(object)failure;
        }
        request.UserId = user.Id;
        return await next();
    }
}
