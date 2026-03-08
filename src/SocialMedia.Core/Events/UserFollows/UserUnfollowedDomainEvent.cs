using SocialMedia.Core.Abstractions;

namespace SocialMedia.Core.Events.UserFollows;

public sealed record UserUnfollowedDomainEvent(
    int FollowerId,
    int FollowingId
) : IDomainEvent;

