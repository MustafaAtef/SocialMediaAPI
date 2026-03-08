using SocialMedia.Core.Abstractions;

namespace SocialMedia.Core.Events.Users;

public sealed record UserUpdatedDomainEvent(
    int UserId,
    string UserName,
    string UserAvatarUrl
) : IDomainEvent;
