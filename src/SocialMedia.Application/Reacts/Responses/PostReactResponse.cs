using SocialMedia.Application.Users.Responses;
using SocialMedia.Core.Enumerations;

namespace SocialMedia.Application.Reacts.Responses;

public class PostReactResponse
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public UserResponse ReactedBy { get; set; } = new();
    public ReactType TypeNo { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
