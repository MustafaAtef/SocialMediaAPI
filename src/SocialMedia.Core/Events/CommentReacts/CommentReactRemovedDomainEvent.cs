using SocialMedia.Core.Abstractions;

namespace SocialMedia.Core.Events.CommentReacts;

public sealed record CommentReactRemovedDomainEvent(
    int Id,
    int CommentId
) : IDomainEvent;

