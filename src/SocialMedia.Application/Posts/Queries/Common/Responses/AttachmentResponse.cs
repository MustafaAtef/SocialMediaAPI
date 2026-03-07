namespace SocialMedia.Application.Posts.Queries.Common.Responses;

public class AttachmentResponse
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}
