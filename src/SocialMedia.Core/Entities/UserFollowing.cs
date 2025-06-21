using System;

namespace SocialMedia.Core.Entities;

public class UserFollowing
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int FollowingId { get; set; }
    public User Following { get; set; }
    public DateTime CreatedAt { get; set; }
}
