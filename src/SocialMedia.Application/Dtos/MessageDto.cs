using System;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Application.Dtos;

public class SendFirstDirectMessageDto
{
    public int ToId { get; set; }
    public string Message { get; set; }
}

public class SendDirectMessageDto
{
    public Guid GroupId { get; set; }
    public string Message { get; set; }
}
public class MessageStatusDto
{
    public UserDto RecievedBy { get; set; }
    public MessageStatusType StatusType { get; set; }
    public string Status { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? SeenAt { get; set; }
}

public class MessageDto
{
    public int Id { get; set; }
    public Guid GroupId { get; set; }
    public UserDto SentBy { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<MessageStatusDto> Status { get; set; }
}

public class GroupMessagesDto
{
    public Guid GroupId { get; set; }
    public string Type { get; set; }
    public string? Name { get; set; }
    public ICollection<UserDto> Members { get; set; }
    public PagedMessagesDto Messages { get; set; }
}

public class DeliveredMessagesDto
{
    public Guid GroudId { get; set; }
    public int RecieverId { get; set; }
}

public class ReadMessagesInGroupDto
{
    public Guid GroupId { get; set; }
    public int RecieverId { get; set; }
}

public class PagedMessagesDto
{
    public Guid GroupId { get; set; }
    public int LastMessageId { get; set; }
    public int PageSize { get; set; }
    public bool HasOlderMessages { get; set; }
    public ICollection<MessageDto> Data { get; set; }
}