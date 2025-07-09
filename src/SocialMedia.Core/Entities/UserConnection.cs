using System;

namespace SocialMedia.Core.Entities;

public class UserConnection
{
    public int UserId { get; set; }
    public string ConnectionId { get; set; }
    public User User { get; set; }
}
