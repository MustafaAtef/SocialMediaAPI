using System;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Core.Entities;

public class MessageStatus
{
    public int MessageId { get; set; }
    public int RecieverId { get; set; }
    public MessageStatusType Status { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? SeenAt { get; set; }
    public Message Message { get; set; }
    public User Reciever { get; set; }

}
