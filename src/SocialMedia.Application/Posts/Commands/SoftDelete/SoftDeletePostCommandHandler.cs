using SocialMedia.Core.RepositoryContracts;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Exceptions;

namespace SocialMedia.Application.Posts.Commands.SoftDelete;

public class SoftDeletePostCommandHandler(IUserService userService, IUnitOfWork unitOfWork) : ICommandHandler<SoftDeletePostCommand>
{
    public async Task<Result> Handle(SoftDeletePostCommand request, CancellationToken cancellationToken)
    {
        var tokenUser = userService.GetAuthenticatedUser();
        if (tokenUser is null) return Result.Failure(UserErrors.Unauthenticated);
        var post = await unitOfWork.Posts.GetAsync(p => p.Id == request.PostId);
        if (post is null) return Result.Failure(PostErrors.NotFound);
        // REFACTOR
        if (post.UserId != tokenUser.Id) throw new UnAuthorizedException("User not authorized to delete this post.");
        post.IsDeleted = true;
        post.DeletedAt = DateTime.Now;
        await unitOfWork.SaveChangesAsync();
        return Result.Success();
    }
}
