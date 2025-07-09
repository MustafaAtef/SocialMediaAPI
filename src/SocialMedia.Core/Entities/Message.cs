using System;

namespace SocialMedia.Core.Entities;

public class Message
{
    public int Id { get; set; }
    public Guid GroupId { get; set; }
    public int FromId { get; set; }
    public string Data { get; set; }
    public DateTime CreatedAt { get; set; }
    public Group Group { get; set; }
    public User FromUser { get; set; }
    public ICollection<MessageStatus> MessageStatuses { get; set; } = new List<MessageStatus>();
}
