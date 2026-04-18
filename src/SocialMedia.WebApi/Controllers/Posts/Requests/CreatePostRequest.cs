
using Microsoft.AspNetCore.Http;

namespace SocialMedia.WebApi.Controllers.Posts.Requests;

public class CreatePostRequest
{
    public string Content { get; set; } = string.Empty;

    public List<IFormFile> Attachments { get; set; } = [];
}
