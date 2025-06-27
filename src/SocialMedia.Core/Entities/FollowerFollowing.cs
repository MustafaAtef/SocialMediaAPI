using System;

namespace SocialMedia.Core.Entities;

public class FollowerFollowing
{
    public int FollowerId { get; set; }
    public int FollowingId { get; set; }

    public DateTime? CreatedAt { get; set; }
    public User Follower { get; set; }
    public User Following { get; set; }
}
