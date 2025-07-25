using System;
using System.ComponentModel.DataAnnotations;

namespace SocialMedia.Application.Dtos;


public class CreateCommentDto
{
    public int PostId { get; set; }
    [Required(ErrorMessage = "Comment content is required.")]
    [MaxLength(500, ErrorMessage = "Comment max length is 500 characters")]
    public string Content { get; set; }
}

public class ReplyCommentDto
{
    public int PostId { get; set; }
    public int ParentCommentId { get; set; }
    [Required(ErrorMessage = "Comment content is required.")]
    [MaxLength(500, ErrorMessage = "Comment max length is 500 characters")]
    public string Content { get; set; }
}

public class UpdateCommentDto
{
    public int CommentId { get; set; }
    public int PostId { get; set; }
    [Required(ErrorMessage = "Comment content is required.")]
    [MaxLength(500, ErrorMessage = "Comment max length is 500 characters")]
    public string Content { get; set; }
}
public class CommentDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string Content { get; set; }
    public UserDto CreatedBy { get; set; }
    public int ReactsCount { get; set; }
    public int RepliesCount { get; set; }
    public PagedList<CommentWithoutRepliesDto>? Replies { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CommentWithoutRepliesDto
{
    public int Id { get; set; }
    public int? ParentCommentId { get; set; }
    public int PostId { get; set; }
    public string Content { get; set; }
    public UserDto CreatedBy { get; set; }
    public int ReactsCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

