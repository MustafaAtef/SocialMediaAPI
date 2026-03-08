using SocialMedia.Core.Abstractions;

namespace SocialMedia.Core.Events.Comments;

public sealed record CommentCreatedDomainEvent(
    int CommentId,
    int PostId,
    int? ParentCommentId,
    int UserId,
    string UserName,
    string UserEmail,
    string UserAvatarUrl,
    string Content,
    DateTime CreatedAt
) : IDomainEvent;

