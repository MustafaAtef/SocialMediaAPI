using System;

namespace SocialMedia.Core.Entities;

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int ReactionsCount { get; set; }
    public int CommentsCount { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public ICollection<PostAttachment>? Attachments { get; set; }
    public ICollection<Comment>? Comments { get; set; }
    public ICollection<PostReact>? Reactions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
