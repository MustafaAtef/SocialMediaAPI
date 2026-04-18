namespace SocialMedia.WebApi.Controllers.Posts.Requests;

public class UpdatePostRequest
{
    public string? Content { get; set; }
    public List<IFormFile>? AddedAttachments { get; set; }
    public List<int>? DeletedAttachmentIds { get; set; }
}
