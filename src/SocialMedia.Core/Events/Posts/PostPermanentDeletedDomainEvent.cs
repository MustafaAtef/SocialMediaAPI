using SocialMedia.Core.Abstractions;

namespace SocialMedia.Core.Events.Posts;

public sealed record PostPermanentDeletedDomainEvent(
    int PostId
) : IDomainEvent;

