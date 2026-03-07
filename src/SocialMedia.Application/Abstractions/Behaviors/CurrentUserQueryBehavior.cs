
using MediatR;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Abstractions.Behaviors;

public class CurrentUserQueryBehavior<TRequest, TResponse>(IUserService userService) : IPipelineBehavior<TRequest, Result<TResponse>>
    where TRequest : ICurrentUserQuery<TResponse>
{

    public async Task<Result<TResponse>> Handle(TRequest request, RequestHandlerDelegate<Result<TResponse>> next, CancellationToken cancellationToken)
    {
        var user = userService.GetAuthenticatedUser();
        if (user is null) return Result.Failure<TResponse>(UserErrors.Unauthenticated);
        request.UserId = user.Id;
        return await next();
    }
}
