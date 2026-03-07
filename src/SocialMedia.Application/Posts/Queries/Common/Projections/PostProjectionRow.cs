namespace SocialMedia.Application.Posts.Queries.Common.Projections;

public class PostProjectionRow
{
    public int PostId { get; init; }
    public string Content { get; init; } = string.Empty;
    public int ReactsCount { get; init; }
    public int CommentsCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? DeletedAt { get; init; }
    public int UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string UserAvatarUrl { get; init; } = string.Empty;
}
