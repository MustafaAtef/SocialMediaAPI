using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Core.Events.PostReacts;

public sealed record PostReactAddedDomainEvent(
    int Id,
    int PostId,
    int UserId,
    string UserName,
    string UserEmail,
    string UserAvatarUrl,
    ReactType ReactType,
    DateTime CreatedAt
) : IDomainEvent;

