using SocialMedia.Core.Abstractions;

namespace SocialMedia.Core.Events.Comments;

public sealed record CommentUpdatedDomainEvent(
    int CommentId,
    string Content,
    DateTime UpdatedAt
) : IDomainEvent;

