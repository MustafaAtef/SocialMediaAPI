using SocialMedia.Core.Abstractions;

namespace SocialMedia.Core.Events.Comments;

public sealed record CommentDeletedDomainEvent(
    int CommentId,
    int PostId,
    int RepliesCount
) : IDomainEvent;

