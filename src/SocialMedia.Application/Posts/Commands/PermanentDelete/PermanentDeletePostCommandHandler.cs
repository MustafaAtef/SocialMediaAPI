using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Exceptions;
using SocialMedia.Core.Events.Posts;

namespace SocialMedia.Application.Posts.Commands.PermanentDelete;

public class PermanentDeletePostCommandHandler(IUserService userService, IUnitOfWork unitOfWork) : ICommandHandler<PermanentDeletePostCommand>
{
    public async Task<Result> Handle(PermanentDeletePostCommand request, CancellationToken cancellationToken)
    {
        var userToken = userService.GetAuthenticatedUser();
        if (userToken is null) return Result.Failure(UserErrors.Unauthenticated);
        var user = await unitOfWork.Users.GetByIdAsync(userToken.Id);
        if (user is null) return Result.Failure(UserErrors.NotFound);

        var post = await unitOfWork.Posts.GetAsync(request.PostId);
        if (post is null) return Result.Failure(PostErrors.NotFound);

        // REFACTOR
        if (post.UserId != userToken.Id) throw new UnAuthorizedException("User not authorized to permanently delete this post.");

        user.RaiseDomainEvent(() => new PostPermanentDeletedDomainEvent(post.Id));
        var success = await unitOfWork.Posts.PermanentDeleteAsync(request.PostId);
        if (!success) throw new BadRequestException("Error happened while deleting the post try again later.");

        return Result.Success();
    }
}