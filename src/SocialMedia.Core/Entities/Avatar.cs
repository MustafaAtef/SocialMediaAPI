
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Core.Entities;

public class Avatar
{
    public int Id { get; set; }
    public StorageProvider StorageProvider { get; set; }
    public string Url { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
}
