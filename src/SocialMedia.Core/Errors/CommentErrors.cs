namespace SocialMedia.Core.Errors;

public static class CommentErrors
{
    public static readonly Error NotFound = new(ErrorType.NotFound, "Comment.NotFound", "The specified comment was not found.");
    public static readonly Error ParentNotFound = new(ErrorType.NotFound, "Comment.ParentNotFound", "The specified parent comment was not found.");
    public static readonly Error ReplyOnReplyNotAllowed = new(ErrorType.Validation, "Comment.ReplyOnReplyNotAllowed", "Replies are not allowed on replies to comments.");
    public static readonly Error Unauthorized = new(ErrorType.Forbidden, "Comment.Unauthorized", "User is not authorized to perform this action on the comment.");
    public static readonly Error DeleteFailed = new(ErrorType.Failure, "Comment.DeleteFailed", "Comment could not be deleted, please try again later.");
}
