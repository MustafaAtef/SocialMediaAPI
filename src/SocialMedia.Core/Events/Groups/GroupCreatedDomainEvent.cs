using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Core.Events.Groups;

public sealed record GroupCreatedDomainEvent(
    Guid GroupId,
    GroupType Type,
    DateTime CreatedAt,
    IReadOnlyList<GroupMemberData> Members
) : IDomainEvent;

public sealed record GroupMemberData(
    int UserId,
    string UserName,
    string UserEmail,
    string UserAvatarUrl
);

