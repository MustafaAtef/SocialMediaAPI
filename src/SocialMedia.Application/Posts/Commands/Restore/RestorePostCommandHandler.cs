using EducationCenter.Core.RepositoryContracts;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Exceptions;

namespace SocialMedia.Application.Posts.Commands.Restore;

public class RestorePostCommandHandler(IUserService userService, IUnitOfWork unitOfWork) : ICommandHandler<RestorePostCommand, UserPostsDto>
{
    public async Task<Result<UserPostsDto>> Handle(RestorePostCommand request, CancellationToken cancellationToken)
    {
        var userToken = userService.GetAuthenticatedUser();
        if (userToken is null) return Result.Failure<UserPostsDto>(UserErrors.Unauthenticated);

        var post = await unitOfWork.Posts.GetDeletedPostAsync(request.PostId);
        if (post is null) return Result.Failure<UserPostsDto>(PostErrors.NotFound);

        // REFACTOR
        if (post.UserId != userToken.Id) throw new UnAuthorizedException("User is not authorized to restore this post.");

        post.IsDeleted = false;
        post.DeletedAt = null;
        await unitOfWork.SaveChangesAsync();

        return new UserPostsDto()
        {
            Id = post.Id,
            Content = post.Content,
            Attachments = post.Attachments?.Select(a => new AttachmentDto
            {
                Id = a.Id,
                Url = a.Url
            }).ToList() ?? new List<AttachmentDto>(),
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            DeletedAt = post.DeletedAt,
            ReactsCount = post.ReactionsCount,
            CommentsCount = post.CommentsCount
        };
    }
}