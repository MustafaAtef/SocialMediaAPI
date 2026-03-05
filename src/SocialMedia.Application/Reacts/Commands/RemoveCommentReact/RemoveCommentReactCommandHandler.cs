using SocialMedia.Core.RepositoryContracts;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Reacts.Commands.RemoveCommentReact;

public class RemoveCommentReactCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveCommentReactCommand>
{
    public async Task<Result> Handle(RemoveCommentReactCommand request, CancellationToken cancellationToken)
    {
        var user = userService.GetAuthenticatedUser();
        if (user is null)
            return Result.Failure(UserErrors.Unauthenticated);

        var comment = await unitOfWork.Comments.GetByIdAsync(request.CommentId);
        if (comment is null)
            return Result.Failure(CommentErrors.NotFound);

        var commentReact = await unitOfWork.CommentReacts.GetAsync(
            cr => cr.CommentId == request.CommentId && cr.UserId == user.Id);
        if (commentReact is null)
            return Result.Failure(ReactErrors.CommentReactNotFound);

        unitOfWork.CommentReacts.Remove(commentReact);
        comment.ReactionsCount--;
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
