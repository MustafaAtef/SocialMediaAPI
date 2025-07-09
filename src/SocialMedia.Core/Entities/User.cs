using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialMedia.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiryTime { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiryTime { get; set; }
    public Avatar? Avatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostReact> PostReacts { get; set; } = new List<PostReact>();
    public ICollection<CommentReact> CommentReacts { get; set; } = new List<CommentReact>();
    public ICollection<FollowerFollowing> Followers { get; set; } = new List<FollowerFollowing>();
    public ICollection<FollowerFollowing> Followings { get; set; } = new List<FollowerFollowing>();
    public ICollection<UserConnection> UserConnections { get; set; } = new List<UserConnection>();
    public ICollection<Group> Groups { get; set; } = new List<Group>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<MessageStatus>? MessageStatuses { get; set; }
}
