namespace SocialMedia.Core.Errors;

public static class CommentErrors
{
    public static readonly Error NotFound = new("Comment.NotFound", "The specified comment was not found.");
    public static readonly Error ParentNotFound = new("Comment.ParentNotFound", "The specified parent comment was not found.");
    public static readonly Error ReplyOnReplyNotAllowed = new("Comment.ReplyOnReplyNotAllowed", "Replies are not allowed on replies to comments.");
    public static readonly Error Unauthorized = new("Comment.Unauthorized", "User is not authorized to perform this action on the comment.");
    public static readonly Error DeleteFailed = new("Comment.DeleteFailed", "Comment could not be deleted, please try again later.");
}
