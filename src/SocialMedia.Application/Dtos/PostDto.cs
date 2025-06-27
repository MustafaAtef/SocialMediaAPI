using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SocialMedia.Application.Dtos;

public class CreatePostDto
{
    [Required(ErrorMessage = "Post content is required.")]
    [MaxLength(1000, ErrorMessage = "Post content max length is 1000 characters.")]
    public string Content { get; set; }
    public List<IFormFile>? Attachments { get; set; }
}

public class UpdatePostDto
{
    public int PostId { get; set; }
    public string Content { get; set; }
    public List<IFormFile>? AddedAttachments { get; set; }
    public List<int>? DeletedAttachmentIds { get; set; }
}
public class PostDto
{
    public int Id { get; set; }
    public string Content { get; set; }
    public List<AttachmentDto> Attachments { get; set; }
    public UserDto CreatedBy { get; set; }
    public int ReactsCount { get; set; }
    public int CommentsCount { get; set; }
    public PagedList<CommentDto>? Comments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UserPostsDto
{
    public int Id { get; set; }
    public string Content { get; set; }
    public List<AttachmentDto> Attachments { get; set; }
    public int ReactsCount { get; set; }
    public int CommentsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}



public class AttachmentDto
{
    public int Id { get; set; }
    public string Url { get; set; }
}
