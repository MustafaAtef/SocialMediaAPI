using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Comments.Responses;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Application.Users.Responses;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Comments;

namespace SocialMedia.Application.Comments.Commands.Update;

public class UpdateCommentCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCommentCommand, CommentResponse>
{
    public async Task<Result<CommentResponse>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var user = userService.GetAuthenticatedUser();
        if (user == null)
            return Result.Failure<CommentResponse>(UserErrors.Unauthenticated);

        var comment = await unitOfWork.Comments.GetAsync(
            c => c.Id == request.CommentId && c.PostId == request.PostId);
        if (comment == null)
            return Result.Failure<CommentResponse>(CommentErrors.NotFound);

        if (comment.UserId != user.Id)
            return Result.Failure<CommentResponse>(CommentErrors.Unauthorized);

        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.Now;
        comment.RaiseDomainEvent(() => new CommentUpdatedDomainEvent(comment.Id, comment.Content, comment.UpdatedAt.Value));
        await unitOfWork.SaveChangesAsync();

        return Result.Success(new CommentResponse
        {
            Id = comment.Id,
            ParentCommentId = comment.ParentCommentId,
            PostId = comment.PostId,
            Content = comment.Content,
            CreatedBy = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            ReactsCount = comment.ReactionsCount,
            RepliesCount = comment.RepliesCount,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        });
    }
}
