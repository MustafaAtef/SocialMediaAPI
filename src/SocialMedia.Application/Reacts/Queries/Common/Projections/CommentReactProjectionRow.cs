using SocialMedia.Core.Enumerations;

namespace SocialMedia.Application.Reacts.Queries.Common.Projections;

public class CommentReactProjectionRow
{
    public int Id { get; init; }
    public int CommentId { get; init; }
    public int ReactedById { get; init; }
    public string ReactedByName { get; init; } = string.Empty;
    public string ReactedByEmail { get; init; } = string.Empty;
    public string ReactedByAvatarUrl { get; init; } = string.Empty;
    public ReactType ReactType { get; init; }
    public DateTime CreatedAt { get; init; }
}
