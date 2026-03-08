using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Core.Events.Posts;

public sealed record PostCreatedDomainEvent(
    int PostId,
    int UserId,
    string UserName,
    string UserEmail,
    string UserAvatarUrl,
    string Content,
    DateTime CreatedAt,
    IReadOnlyList<PostAttachmentData> Attachments
) : IDomainEvent;

public sealed record PostAttachmentData(int AttachmentId, string Url, AttachmentType AttachmentType);

