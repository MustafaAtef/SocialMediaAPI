using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Reacts.Responses;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Application.Users.Responses;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.CommentReacts;

namespace SocialMedia.Application.Reacts.Commands.ReactToComment;

public class ReactToCommentCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    : ICommandHandler<ReactToCommentCommand, CommentReactResponse>
{
    public async Task<Result<CommentReactResponse>> Handle(ReactToCommentCommand request, CancellationToken cancellationToken)
    {
        var user = userService.GetAuthenticatedUser();
        if (user is null)
            return Result.Failure<CommentReactResponse>(UserErrors.Unauthenticated);

        var post = await unitOfWork.Posts.GetByIdAsync(request.PostId);
        if (post is null)
            return Result.Failure<CommentReactResponse>(PostErrors.NotFound);

        var comment = await unitOfWork.Comments.GetByIdAsync(request.CommentId);
        if (comment is null)
            return Result.Failure<CommentReactResponse>(CommentErrors.NotFound);

        var commentReact = await unitOfWork.CommentReacts.GetAsync(
            cr => cr.CommentId == request.CommentId && cr.UserId == user.Id);

        if (commentReact != null)
        {
            commentReact.ReactType = request.ReactType;
            commentReact.RaiseDomainEvent(() => new CommentReactUpdatedDomainEvent(commentReact.Id, commentReact.ReactType));
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
            commentReact.RaiseDomainEvent(() => new CommentReactAddedDomainEvent(
                commentReact.Id, commentReact.CommentId, user.Id,
                user.Name, user.Email, user.AvatarUrl,
                commentReact.ReactType, commentReact.CreatedAt));
        }

        await unitOfWork.SaveChangesAsync();

        return Result.Success(new CommentReactResponse
        {
            Id = commentReact.Id,
            CommentId = commentReact.CommentId,
            ReactedBy = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            TypeNo = commentReact.ReactType,
            TypeName = commentReact.ReactType.ToString(),
            CreatedAt = commentReact.CreatedAt
        });
    }
}
