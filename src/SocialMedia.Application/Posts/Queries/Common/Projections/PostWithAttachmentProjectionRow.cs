namespace SocialMedia.Application.Posts.Queries.Common.Projections;

public class PostWithAttachmentProjectionRow : PostProjectionRow
{
    public int? AttachmentId { get; init; }
    public string? AttachmentUrl { get; init; }
    public string? AttachmentType { get; init; }
}
