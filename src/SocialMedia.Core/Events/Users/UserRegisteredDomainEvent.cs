using SocialMedia.Core.Abstractions;

namespace SocialMedia.Core.Events.Users;

public sealed record UserRegisteredDomainEvent(
    int UserId,
    string UserName,
    string UserEmail,
    string UserAvatarUrl
) : IDomainEvent;

