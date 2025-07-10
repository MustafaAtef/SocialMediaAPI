using System;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Application.Dtos;

public class CreateFirstDirectMessageDto
{
    public int ToId { get; set; }
    public string Message { get; set; }
}

public class CreateDirectMessageDto
{
    public Guid GroupId { get; set; }
    public string Message { get; set; }
}

public class DirectMessageDto
{
    public int Id { get; set; }
    public Guid GroupId { get; set; }
    public UserDto FromUser { get; set; }
    public MessageStatusType StatusType { get; set; }
    public string Status { get; set; }

}