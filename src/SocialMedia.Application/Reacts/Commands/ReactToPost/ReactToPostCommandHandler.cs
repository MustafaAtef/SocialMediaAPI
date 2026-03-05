using SocialMedia.Core.RepositoryContracts;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Reacts.Commands.ReactToPost;

public class ReactToPostCommandHandler(IUserService userService, IUnitOfWork unitOfWork)
    : ICommandHandler<ReactToPostCommand, PostReactDto>
{
    public async Task<Result<PostReactDto>> Handle(ReactToPostCommand request, CancellationToken cancellationToken)
    {
        var user = userService.GetAuthenticatedUser();
        if (user == null)
            return Result.Failure<PostReactDto>(UserErrors.Unauthenticated);

        var post = await unitOfWork.Posts.GetByIdAsync(request.PostId);
        if (post == null)
            return Result.Failure<PostReactDto>(PostErrors.NotFound);

        var postReact = await unitOfWork.PostReacts.GetAsync(pr => pr.PostId == request.PostId && pr.UserId == user.Id);
        if (postReact != null)
        {
            postReact.ReactType = request.ReactType;
        }
        else
        {
            postReact = new PostReact
            {
                PostId = request.PostId,
                UserId = user.Id,
                ReactType = request.ReactType,
                CreatedAt = DateTime.UtcNow
            };
            post.ReactionsCount++;
            unitOfWork.PostReacts.Add(postReact);
        }

        await unitOfWork.SaveChangesAsync();

        return Result.Success(new PostReactDto
        {
            Id = postReact.Id,
            PostId = postReact.PostId,
            ReactedBy = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            ReactType = postReact.ReactType,
            Name = postReact.ReactType.ToString(),
            CreatedAt = postReact.CreatedAt
        });
    }
}
