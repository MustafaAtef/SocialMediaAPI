using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Comments;

namespace SocialMedia.Application.Comments.Commands.Create;

public class CreateCommentCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCommentCommand, CommentWithoutRepliesDto>
{
    public async Task<Result<CommentWithoutRepliesDto>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var user = userService.GetAuthenticatedUser();
        if (user == null)
            return Result.Failure<CommentWithoutRepliesDto>(UserErrors.Unauthenticated);

        var post = await unitOfWork.Posts.GetAsync(p => p.Id == request.PostId);
        if (post == null)
            return Result.Failure<CommentWithoutRepliesDto>(PostErrors.NotFound);

        var comment = new Comment
        {
            Content = request.Content,
            UserId = user.Id,
            PostId = request.PostId,
            ReactionsCount = 0,
            RepliesCount = 0
        };

        post.CommentsCount++;
        unitOfWork.Comments.Add(comment);
        comment.RaiseDomainEvent(() => new CommentCreatedDomainEvent(
            comment.Id, comment.PostId, comment.ParentCommentId,
            user.Id, user.Name, user.Email, user.AvatarUrl,
            comment.Content, comment.CreatedAt));
        await unitOfWork.SaveChangesAsync();

        return Result.Success(new CommentWithoutRepliesDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            CreatedBy = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            Content = comment.Content,
            ReactsCount = 0,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        });
    }
}
