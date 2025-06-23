using System;

namespace SocialMedia.Application.Dtos;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string AvatarUrl { get; set; }
}
