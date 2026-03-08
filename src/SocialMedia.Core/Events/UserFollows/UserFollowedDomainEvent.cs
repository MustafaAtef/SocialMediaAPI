using SocialMedia.Core.Abstractions;

namespace SocialMedia.Core.Events.UserFollows;

public sealed record UserFollowedDomainEvent(
    int FollowerId,
    int FollowingId,
    string FollowerName,
    string FollowerEmail,
    string FollowerAvatarUrl,
    string FollowingName,
    string FollowingEmail,
    string FollowingAvatarUrl,
    DateTime CreatedAt
) : IDomainEvent;

