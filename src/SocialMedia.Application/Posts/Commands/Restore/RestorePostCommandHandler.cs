using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Posts.Responses;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Application.Users.Responses;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Exceptions;
using SocialMedia.Core.Events.Posts;

namespace SocialMedia.Application.Posts.Commands.Restore;

public class RestorePostCommandHandler(IUserService userService, IUnitOfWork unitOfWork) : ICommandHandler<RestorePostCommand, PostResponse>
{
    public async Task<Result<PostResponse>> Handle(RestorePostCommand request, CancellationToken cancellationToken)
    {
        var userToken = userService.GetAuthenticatedUser();
        if (userToken is null) return Result.Failure<PostResponse>(UserErrors.Unauthenticated);

        var post = await unitOfWork.Posts.GetDeletedPostAsync(request.PostId);
        if (post is null) return Result.Failure<PostResponse>(PostErrors.NotFound);

        // REFACTOR
        if (post.UserId != userToken.Id) throw new UnAuthorizedException("User is not authorized to restore this post.");

        post.IsDeleted = false;
        post.DeletedAt = null;
        post.RaiseDomainEvent(() => new PostRestoredDomainEvent(post.Id));
        await unitOfWork.SaveChangesAsync();

        return new PostResponse()
        {
            Id = post.Id,
            Content = post.Content,
            Attachments = post.Attachments?.Select(a => new AttachmentResponse
            {
                Id = a.Id,
                Url = a.Url,
            }).ToList() ?? new List<AttachmentResponse>(),
            Author = new UserResponse
            {
                Id = userToken.Id,
                Name = userToken.Name,
                Email = userToken.Email,
                AvatarUrl = userToken.AvatarUrl
            },
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            DeletedAt = post.DeletedAt,
            ReactsCount = post.ReactionsCount,
            CommentsCount = post.CommentsCount
        };
    }
}