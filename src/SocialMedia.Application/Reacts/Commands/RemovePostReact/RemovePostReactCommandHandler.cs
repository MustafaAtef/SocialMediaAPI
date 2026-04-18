using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.PostReacts;

namespace SocialMedia.Application.Reacts.Commands.RemovePostReact;

public class RemovePostReactCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    : ICommandHandler<RemovePostReactCommand>
{
    public async Task<Result> Handle(RemovePostReactCommand request, CancellationToken cancellationToken)
    {
        var user = userService.GetAuthenticatedUser();
        if (user is null)
            return Result.Failure(UserErrors.Unauthenticated);

        var post = await unitOfWork.Posts.GetByIdAsync(request.PostId);
        if (post is null)
            return Result.Failure(PostErrors.NotFound);

        var postReact = await unitOfWork.PostReacts.GetAsync(pr => pr.PostId == request.PostId && pr.UserId == user.Id);
        if (postReact is null)
            return Result.Failure(ReactErrors.PostReactNotFound);

        if (postReact.UserId != user.Id)
            return Result.Failure(ReactErrors.Unauthorized);

        unitOfWork.PostReacts.Remove(postReact);
        post.ReactionsCount--;
        post.RaiseDomainEvent(() => new PostReactRemovedDomainEvent(postReact.Id, postReact.PostId));
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
