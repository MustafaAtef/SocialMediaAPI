using SocialMedia.Core.RepositoryContracts;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Enumerations;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Posts.Commands.Create;

public class CreatePostCommandHandler(IUserService userService, IFileUploader fileUploader, IUnitOfWork unitOfWork) : ICommandHandler<CreatePostCommand, PostDto>
{
    public async Task<Result<PostDto>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var user = userService.GetAuthenticatedUser();
        if (user == null)
        {
            return Result.Failure<PostDto>(UserErrors.Unauthenticated);
        }
        var post = new Post
        {
            Content = request.Content,
            UserId = user.Id,
            Attachments = new List<PostAttachment>()
        };
        foreach (var attachment in request.Attachments)
        {
            (StorageProvider storageType, AttachmentType attachmentType, string url) = await fileUploader.UploadAsync(attachment, "posts-attachments");
            post.Attachments.Add(new PostAttachment
            {
                AttachmentType = attachmentType,
                Url = url,
                StorageProvider = storageType
            });
        }
        unitOfWork.Posts.Add(post);
        await unitOfWork.SaveChangesAsync();
        return new PostDto
        {
            Id = post.Id,
            Content = post.Content,
            Attachments = post.Attachments.Select(a => new AttachmentDto()
            {
                Id = a.Id,
                Url = a.Url
            }).ToList(),
            CreatedBy = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            ReactsCount = 0,
            CommentsCount = 0,
            Comments = null,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }
}
