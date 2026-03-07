namespace SocialMedia.Application.Comments.Queries.Common.Responses;

public class RepliesResponse : CommentResponse
{
    public int ParentCommentId { get; set; }
}