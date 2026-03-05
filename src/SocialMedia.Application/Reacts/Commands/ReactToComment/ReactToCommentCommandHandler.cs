using SocialMedia.Core.RepositoryContracts;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Reacts.Commands.ReactToComment;

public class ReactToCommentCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    : ICommandHandler<ReactToCommentCommand, CommentReactDto>
{
    public async Task<Result<CommentReactDto>> Handle(ReactToCommentCommand request, CancellationToken cancellationToken)
    {
        var user = userService.GetAuthenticatedUser();
        if (user is null)
            return Result.Failure<CommentReactDto>(UserErrors.Unauthenticated);

        var post = await unitOfWork.Posts.GetByIdAsync(request.PostId);
        if (post is null)
            return Result.Failure<CommentReactDto>(PostErrors.NotFound);

        var comment = await unitOfWork.Comments.GetByIdAsync(request.CommentId);
        if (comment is null)
            return Result.Failure<CommentReactDto>(CommentErrors.NotFound);

        var commentReact = await unitOfWork.CommentReacts.GetAsync(
            cr => cr.CommentId == request.CommentId && cr.UserId == user.Id);

        if (commentReact != null)
        {
            commentReact.ReactType = request.ReactType;
        }
        else
        {
            commentReact = new CommentReact
            {
                CommentId = request.CommentId,
                UserId = user.Id,
                ReactType = request.ReactType,
                CreatedAt = DateTime.UtcNow
            };
            comment.ReactionsCount++;
            unitOfWork.CommentReacts.Add(commentReact);
        }

        await unitOfWork.SaveChangesAsync();

        return Result.Success(new CommentReactDto
        {
            Id = commentReact.Id,
            CommentId = commentReact.CommentId,
            ReactedBy = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            ReactType = commentReact.ReactType,
            Name = commentReact.ReactType.ToString(),
            CreatedAt = commentReact.CreatedAt
        });
    }
}
