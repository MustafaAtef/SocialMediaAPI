namespace SocialMedia.Core.Errors;

public static class PostErrors
{
    public static readonly Error NotFound = new(ErrorType.NotFound, "Post.NotFound", "The specified post was not found.");
}
