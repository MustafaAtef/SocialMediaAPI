using SocialMedia.Core.Abstractions;

namespace SocialMedia.Core.Events.Posts;

public sealed record PostSoftDeletedDomainEvent(
    int PostId,
    DateTime DeletedAt
) : IDomainEvent;

