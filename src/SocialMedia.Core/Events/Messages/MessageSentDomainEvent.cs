using SocialMedia.Core.Abstractions;

namespace SocialMedia.Core.Events.Messages;

public sealed record MessageSentDomainEvent(
    int MessageId,
    Guid GroupId,
    int FromId,
    string SenderName,
    string SenderEmail,
    string SenderAvatarUrl,
    string Data,
    DateTime CreatedAt,
    IReadOnlyList<MessageStatusData> Statuses
) : IDomainEvent;

public sealed record MessageStatusData(
    int ReceiverId,
    string ReceiverName,
    string ReceiverEmail,
    string ReceiverAvatarUrl,
    int StatusType,
    DateTime? SentAt,
    DateTime? DeliveredAt,
    DateTime? SeenAt
);

