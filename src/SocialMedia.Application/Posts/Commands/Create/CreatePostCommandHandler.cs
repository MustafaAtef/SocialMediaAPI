using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Posts.Responses;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Application.Users.Responses;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Enumerations;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Posts;

namespace SocialMedia.Application.Posts.Commands.Create;

public class CreatePostCommandHandler(IUserService userService, IFileUploader fileUploader, IUnitOfWork unitOfWork) : ICommandHandler<CreatePostCommand, PostResponse>
{
    public async Task<Result<PostResponse>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var user = userService.GetAuthenticatedUser();
        if (user == null)
            return Result.Failure<PostResponse>(UserErrors.Unauthenticated);

        var post = new Post
        {
            Content = request.Content,
            UserId = user.Id,
            Attachments = new List<PostAttachment>()
        };
        foreach (var attachment in request.Attachments)
        {
            var uploadedAttachment = await fileUploader.UploadAsync(attachment, "posts-attachments");
            post.Attachments.Add(new PostAttachment
            {
                AttachmentType = uploadedAttachment.Type,
                StorageProvider = uploadedAttachment.StorageProvider,
                Url = uploadedAttachment.Url
            });
        }
        unitOfWork.Posts.Add(post);
        post.RaiseDomainEvent(() => new PostCreatedDomainEvent(
            post.Id,
            user.Id,
            user.Name,
            user.Email,
            user.AvatarUrl,
            post.Content,
            post.CreatedAt,
            post.Attachments.Select(a => new PostAttachmentData(a.Id, a.Url, a.AttachmentType)).ToList()
        ));
        await unitOfWork.SaveChangesAsync();

        return new PostResponse
        {
            Id = post.Id,
            Content = post.Content,
            Attachments = post.Attachments.Select(a => new AttachmentResponse()
            {
                Id = a.Id,
                Url = a.Url,
            }).ToList(),
            Author = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            ReactsCount = 0,
            CommentsCount = 0,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }
}
