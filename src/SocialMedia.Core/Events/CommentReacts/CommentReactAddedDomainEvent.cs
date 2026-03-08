using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Core.Events.CommentReacts;

public sealed record CommentReactAddedDomainEvent(
    int Id,
    int CommentId,
    int UserId,
    string UserName,
    string UserEmail,
    string UserAvatarUrl,
    ReactType ReactType,
    DateTime CreatedAt
) : IDomainEvent;

