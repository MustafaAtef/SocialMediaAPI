using System;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Core.Entities;

public class Group
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public GroupType Type { get; set; }
    public DateTime CreatedAT { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
