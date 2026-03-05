using SocialMedia.Core.RepositoryContracts;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Comments.Commands.Update;

public class UpdateCommentCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCommentCommand, CommentDto>
{
    public async Task<Result<CommentDto>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var user = userService.GetAuthenticatedUser();
        if (user == null)
            return Result.Failure<CommentDto>(UserErrors.Unauthenticated);

        var comment = await unitOfWork.Comments.GetAsync(
            c => c.Id == request.CommentId && c.PostId == request.PostId);
        if (comment == null)
            return Result.Failure<CommentDto>(CommentErrors.NotFound);

        if (comment.UserId != user.Id)
            return Result.Failure<CommentDto>(CommentErrors.Unauthorized);

        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.Now;
        await unitOfWork.SaveChangesAsync();

        return Result.Success(new CommentDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            Content = comment.Content,
            CreatedBy = new UserDto
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
