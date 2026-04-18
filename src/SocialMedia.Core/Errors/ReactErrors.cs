namespace SocialMedia.Core.Errors;

public static class ReactErrors
{
    public static readonly Error PostReactNotFound = new(ErrorType.NotFound, "React.PostReactNotFound", "The specified post react was not found.");
    public static readonly Error CommentReactNotFound = new(ErrorType.NotFound, "React.CommentReactNotFound", "The specified comment react was not found.");
    public static readonly Error Unauthorized = new(ErrorType.Forbidden, "React.Unauthorized", "User is not authorized to perform this action on the react.");
}
