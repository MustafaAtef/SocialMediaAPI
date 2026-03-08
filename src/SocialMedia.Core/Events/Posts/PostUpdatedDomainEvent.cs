using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Core.Events.Posts;

public sealed record PostUpdatedDomainEvent(
    int PostId,
    string Content,
    DateTime UpdatedAt,
    IReadOnlyList<PostAttachmentData> AddedAttachments,
    IReadOnlyList<int> RemovedAttachmentIds
) : IDomainEvent;

