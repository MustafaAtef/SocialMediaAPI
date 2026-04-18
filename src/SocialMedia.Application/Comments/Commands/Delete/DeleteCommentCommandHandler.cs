using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Comments;

namespace SocialMedia.Application.Comments.Commands.Delete;

public class DeleteCommentCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCommentCommand>
{
    public async Task<Result> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var user = userService.GetAuthenticatedUser();
        if (user is null)
            return Result.Failure(UserErrors.Unauthenticated);

        var post = await unitOfWork.Posts.GetAsync(p => p.Id == request.PostId);
        if (post is null)
            return Result.Failure(PostErrors.NotFound);

        var comment = await unitOfWork.Comments.GetAsync(
            c => c.Id == request.CommentId && c.PostId == request.PostId, ["Replies"]);
        if (comment is null)
            return Result.Failure(CommentErrors.NotFound);

        if (comment.UserId != user.Id)
            return Result.Failure(CommentErrors.Unauthorized);

        if (comment.Replies is not null && comment.RepliesCount > 0)
            unitOfWork.Comments.RemoveRange(comment.Replies);
        else
        {
            // decrease the replies count of the parent comment it exists because we don't have multiple levels of comments
            var parentComment = await unitOfWork.Comments.GetAsync(c => c.Id == comment.ParentCommentId);
            if (parentComment != null)
            {
                parentComment.RepliesCount--;
            }
        }

        unitOfWork.Comments.Remove(comment);
        post.CommentsCount -= 1 + comment.RepliesCount;
        post.RaiseDomainEvent(() => new CommentDeletedDomainEvent(comment.Id, comment.PostId, comment.RepliesCount));

        var entitiesDeleted = await unitOfWork.SaveChangesAsync();
        if (entitiesDeleted == 0)
            return Result.Failure(CommentErrors.DeleteFailed);

        return Result.Success();
    }
}
