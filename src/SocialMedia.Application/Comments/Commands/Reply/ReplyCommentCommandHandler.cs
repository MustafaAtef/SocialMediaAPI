using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Comments.Responses;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Application.Users.Responses;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Comments;

namespace SocialMedia.Application.Comments.Commands.Reply;

public class ReplyCommentCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    : ICommandHandler<ReplyCommentCommand, CommentResponse>
{
    public async Task<Result<CommentResponse>> Handle(ReplyCommentCommand request, CancellationToken cancellationToken)
    {
        var user = userService.GetAuthenticatedUser();
        if (user == null)
            return Result.Failure<CommentResponse>(UserErrors.Unauthenticated);

        var post = await unitOfWork.Posts.GetAsync(p => p.Id == request.PostId);
        if (post == null)
            return Result.Failure<CommentResponse>(PostErrors.NotFound);

        var parentComment = await unitOfWork.Comments.GetAsync(
            c => c.Id == request.ParentCommentId && c.PostId == request.PostId);
        if (parentComment == null)
            return Result.Failure<CommentResponse>(CommentErrors.ParentNotFound);

        if (parentComment.ParentCommentId is not null)
            return Result.Failure<CommentResponse>(CommentErrors.ReplyOnReplyNotAllowed);

        var reply = new Comment
        {
            Content = request.Content,
            UserId = user.Id,
            PostId = request.PostId,
            ParentCommentId = request.ParentCommentId,
            ReactionsCount = 0,
            RepliesCount = 0
        };

        parentComment.RepliesCount++;
        post.CommentsCount++;
        unitOfWork.Comments.Add(reply);
        reply.RaiseDomainEvent(() => new CommentCreatedDomainEvent(
            reply.Id, reply.PostId, reply.ParentCommentId,
            user.Id, user.Name, user.Email, user.AvatarUrl,
            reply.Content, reply.CreatedAt));
        await unitOfWork.SaveChangesAsync();

        return Result.Success(new CommentResponse
        {
            Id = reply.Id,
            ParentCommentId = parentComment.Id,
            PostId = reply.PostId,
            CreatedBy = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            Content = reply.Content,
            ReactsCount = 0,
            RepliesCount = 0,
            CreatedAt = reply.CreatedAt,
            UpdatedAt = reply.UpdatedAt
        });
    }
}
