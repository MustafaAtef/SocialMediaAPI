using SocialMedia.Core.Abstractions;

namespace SocialMedia.Core.Events.Users;

public sealed record UserCreatedDomainEvent(
    int UserId,
    string UserName,
    string UserEmail,
    string UserAvatarUrl
) : IDomainEvent;

