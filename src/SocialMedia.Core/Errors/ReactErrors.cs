namespace SocialMedia.Core.Errors;

public static class ReactErrors
{
    public static readonly Error PostReactNotFound = new("React.PostReactNotFound", "The specified post react was not found.");
    public static readonly Error CommentReactNotFound = new("React.CommentReactNotFound", "The specified comment react was not found.");
    public static readonly Error Unauthorized = new("React.Unauthorized", "User is not authorized to perform this action on the react.");
}
