using System;
using Microsoft.AspNetCore.Http;

namespace SocialMedia.Application.Dtos;

public class CreatePostDto
{
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
    public ICollection<CommentDto> Comments { get; set; } = new List<CommentDto>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AttachmentDto
{
    public int Id { get; set; }
    public string Url { get; set; }
}
