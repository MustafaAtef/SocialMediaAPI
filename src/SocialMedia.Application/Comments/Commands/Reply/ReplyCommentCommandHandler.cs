using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Comments;

namespace SocialMedia.Application.Comments.Commands.Reply;

public class ReplyCommentCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    : ICommandHandler<ReplyCommentCommand, CommentWithoutRepliesDto>
{
    public async Task<Result<CommentWithoutRepliesDto>> Handle(ReplyCommentCommand request, CancellationToken cancellationToken)
    {
        var user = userService.GetAuthenticatedUser();
        if (user == null)
            return Result.Failure<CommentWithoutRepliesDto>(UserErrors.Unauthenticated);

        var post = await unitOfWork.Posts.GetAsync(p => p.Id == request.PostId);
        if (post == null)
            return Result.Failure<CommentWithoutRepliesDto>(PostErrors.NotFound);

        var parentComment = await unitOfWork.Comments.GetAsync(
            c => c.Id == request.ParentCommentId && c.PostId == request.PostId);
        if (parentComment == null)
            return Result.Failure<CommentWithoutRepliesDto>(CommentErrors.ParentNotFound);

        if (parentComment.ParentComment is not null)
            return Result.Failure<CommentWithoutRepliesDto>(CommentErrors.ReplyOnReplyNotAllowed);

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

        return Result.Success(new CommentWithoutRepliesDto
        {
            Id = reply.Id,
            ParentCommentId = parentComment.Id,
            PostId = reply.PostId,
            CreatedBy = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            Content = reply.Content,
            ReactsCount = 0,
            CreatedAt = reply.CreatedAt,
            UpdatedAt = reply.UpdatedAt
        });
    }
}
