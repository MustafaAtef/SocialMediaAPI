using System;

namespace SocialMedia.Core.Entities;

public class UserFollower
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int FollowerId { get; set; }
    public User Follower { get; set; }
    public DateTime CreatedAt { get; set; }

}
