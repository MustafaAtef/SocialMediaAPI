using SocialMedia.Core.Abstractions;

namespace SocialMedia.Core.Events.Posts;

public sealed record PostRestoredDomainEvent(
    int PostId
) : IDomainEvent;

