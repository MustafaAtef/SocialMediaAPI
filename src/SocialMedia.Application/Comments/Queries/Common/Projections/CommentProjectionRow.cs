namespace SocialMedia.Application.Comments.Queries.Common.Projections;

public class CommentProjectionRow
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public int PostId { get; init; }
    public int? ParentCommentId { get; init; }
    public string Content { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string UserEmail { get; init; } = string.Empty;
    public string UserAvatarUrl { get; init; } = string.Empty;
    public int ReactsCount { get; init; }
    public int RepliesCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
