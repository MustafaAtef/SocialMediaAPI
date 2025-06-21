using System;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Core.Entities;

public class PostReact
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public Post Post { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
    public ReactType ReactType { get; set; }
    public DateTime CreatedAt { get; set; }
}
