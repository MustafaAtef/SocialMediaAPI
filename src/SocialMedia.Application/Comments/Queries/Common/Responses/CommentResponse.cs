using SocialMedia.Application.Users.Common.Responses;

namespace SocialMedia.Application.Comments.Queries.Common.Responses;

public class CommentResponse
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string Content { get; set; } = string.Empty;
    public UserResponse CreatedBy { get; set; } = new();
    public int ReactsCount { get; set; }
    public int RepliesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}