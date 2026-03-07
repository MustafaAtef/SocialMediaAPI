using SocialMedia.Application.Users.Common.Responses;

namespace SocialMedia.Application.Posts.Queries.Common.Responses;

public class PostResponse
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<AttachmentResponse> Attachments { get; set; } = new();
    public UserResponse Author { get; set; } = new();
    public int ReactsCount { get; set; }
    public int CommentsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
