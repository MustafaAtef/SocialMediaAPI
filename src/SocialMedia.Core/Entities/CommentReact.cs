using SocialMedia.Core.Enumerations;

namespace SocialMedia.Core.Entities;

public class CommentReact
{
    public int Id { get; set; }
    public int CommentId { get; set; }
    public Comment Comment { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
    public ReactType ReactType { get; set; }
    public DateTime CreatedAt { get; set; }
}
