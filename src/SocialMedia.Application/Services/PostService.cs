using System;
using System.Security.Claims;
using EducationCenter.Core.RepositoryContracts;
using Microsoft.AspNetCore.Http;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Application.Services;

public class PostService : IPostService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IFileUploader _fileUploader;
    private readonly IUserService _userService;
    public PostService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, IFileUploader fileUploader, IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _fileUploader = fileUploader;
        _userService = userService;
    }
    public async Task<PostDto> CreateAsync(CreatePostDto createPostDto)
    {
        var user = _userService.GetAuthenticatedUser();
        if (user == null)
        {
            throw new Exception("User is not authenticated.");
        }
        var post = new Post
        {
            Content = createPostDto.Content,
            UserId = user.Id,
            Attachments = new List<PostAttachment>()
        };
        foreach (var attachment in createPostDto.Attachments)
        {
            (StorageProvider storageType, AttachmentType attachmentType, string url) = await _fileUploader.UploadAsync(attachment, "posts-attachments");
            post.Attachments.Add(new PostAttachment
            {
                AttachmentType = attachmentType,
                Url = url,
                StorageProvider = storageType
            });
        }
        _unitOfWork.Posts.Add(post);
        await _unitOfWork.SaveChangesAsync();
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
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }

    public async Task<PostDto> UpdateAsync(UpdatePostDto updatePostDto)
    {
        // check if the logged in user is the owner of the post
        var user = _userService.GetAuthenticatedUser();
        if (user == null)
        {
            throw new Exception("User is not authenticated.");
        }
        var post = await _unitOfWork.Posts.GetAsync(p => p.Id == updatePostDto.PostId, ["Attachments"]);
        if (post == null)
        {
            throw new Exception("Post not found.");
        }
        if (user.Id != post.UserId)
        {
            throw new Exception("You are not authorized to update this post.");
        }
        // update the content of the post then delete all deleted attachments ids and insert any new attachments 
        if (updatePostDto.Content is not null) post.Content = updatePostDto.Content;
        post.UpdatedAt = DateTime.Now;

        if (updatePostDto.AddedAttachments != null)
        {
            foreach (var attachment in updatePostDto.AddedAttachments)
            {
                (StorageProvider storageType, AttachmentType attachmentType, string url) = await _fileUploader.UploadAsync(attachment, "posts-attachments");
                post.Attachments?.Add(new PostAttachment
                {
                    AttachmentType = attachmentType,
                    Url = url,
                    StorageProvider = storageType
                });
            }
        }
        await _unitOfWork.SaveChangesAsync();
        if (updatePostDto.DeletedAttachmentIds != null)
        {
            foreach (var attachmentId in updatePostDto.DeletedAttachmentIds)
            {
                var attachment = post.Attachments?.FirstOrDefault(a => a.Id == attachmentId);
                if (attachment != null)
                {
                    post.Attachments?.Remove(attachment);
                    await _fileUploader.DeleteAsync(attachment.Url);
                }
            }
        }
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
            ReactsCount = post.ReactionsCount,
            CommentsCount = post.CommentsCount,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }
}
