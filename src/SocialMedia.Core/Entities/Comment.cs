using System;

namespace SocialMedia.Core.Entities;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int ReactionsCount { get; set; }
    public int RepliesCount { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int PostId { get; set; }
    public Post Post { get; set; }
    public int? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; }
    public ICollection<CommentReact>? Reactions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
