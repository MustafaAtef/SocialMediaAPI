using System;

namespace SocialMedia.Application.Dtos;

public class FirstDirectMessageDto
{
    public int FromId { get; set; }
    public int ToId { get; set; }
    public string Message { get; set; }
}

public class DirectMessageDto
{
    public int GroupId { get; set; }
    public int FromId { get; set; }
    public string Message { get; set; }
}