using SocialMedia.Core.RepositoryContracts;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Posts.Responses;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Application.Users.Responses;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Enumerations;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Exceptions;
using SocialMedia.Core.Events.Posts;

namespace SocialMedia.Application.Posts.Commands.Update;

public class UpdatePostCommandHandler(IUserService userService, IFileUploader fileUploader, IUnitOfWork unitOfWork) : ICommandHandler<UpdatePostCommand, PostResponse>
{
    public async Task<Result<PostResponse>> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        // check if the logged in user is the owner of the post
        var user = userService.GetAuthenticatedUser();
        if (user == null)
        {
            return Result.Failure<PostResponse>(UserErrors.Unauthenticated);
        }
        var post = await unitOfWork.Posts.GetAsync(p => p.Id == request.PostId, ["Attachments"]);
        if (post == null)
        {
            return Result.Failure<PostResponse>(PostErrors.NotFound);
        }
        if (user.Id != post.UserId)
        {
            // REFACTOR 
            throw new UnAuthorizedException("User not authorized to update this post.");
        }

        // update the content of the post then delete all deleted attachments ids and insert any new attachments 
        if (request.Content is not null) post.Content = request.Content;
        post.UpdatedAt = DateTime.Now;

        var addedAttachments = new List<PostAttachment>();
        if (request.AddedAttachments != null)
        {
            foreach (var attachment in request.AddedAttachments)
            {
                (StorageProvider storageType, AttachmentType attachmentType, string url) = await fileUploader.UploadAsync(attachment, "posts-attachments");
                var newAttachment = new PostAttachment
                {
                    AttachmentType = attachmentType,
                    Url = url,
                    StorageProvider = storageType
                };
                post.Attachments?.Add(newAttachment);
                addedAttachments.Add(newAttachment);
            }
        }

        var removedAttachmentIds = new List<int>();
        if (request.DeletedAttachmentIds != null)
        {
            foreach (var attachmentId in request.DeletedAttachmentIds)
            {
                var attachment = post.Attachments?.FirstOrDefault(a => a.Id == attachmentId);
                if (attachment != null)
                {
                    post.Attachments?.Remove(attachment);
                    removedAttachmentIds.Add(attachmentId);
                    await fileUploader.DeleteAsync(attachment.Url);
                }
            }
        }

        post.RaiseDomainEvent(() => new PostUpdatedDomainEvent(
            post.Id,
            post.Content,
            post.UpdatedAt.Value,
            addedAttachments.Select(a => new PostAttachmentData(a.Id, a.Url, a.AttachmentType)).ToList(),
            removedAttachmentIds
        ));
        await unitOfWork.SaveChangesAsync();

        return new PostResponse
        {
            Id = post.Id,
            Content = post.Content,
            Attachments = post.Attachments?.Select(a => new AttachmentResponse()
            {
                Id = a.Id,
                Url = a.Url,
                Type = a.AttachmentType.ToString()
            }).ToList() ?? new List<AttachmentResponse>(),
            Author = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            },
            ReactsCount = post.ReactionsCount,
            CommentsCount = post.CommentsCount,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }
}
