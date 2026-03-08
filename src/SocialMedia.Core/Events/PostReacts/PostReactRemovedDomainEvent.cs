using SocialMedia.Core.Abstractions;

namespace SocialMedia.Core.Events.PostReacts;

public sealed record PostReactRemovedDomainEvent(
    int Id,
    int PostId
) : IDomainEvent;

