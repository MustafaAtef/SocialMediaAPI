
namespace SocialMedia.Core.Errors;

public static class UserErrors
{
    public static readonly Error NotFound = new(ErrorType.NotFound, "User.NotFound", "The user was not found.");

    public static readonly Error AlreadyExists = new(ErrorType.Conflict, "User.AlreadyExists", "A user with the same email already exists.");

    public static readonly Error InvalidCredentials = new(ErrorType.Validation, "User.InvalidCredentials", "The provided credentials are invalid.");

    public static readonly Error Unauthenticated = new(ErrorType.Unauthorized, "User.Unauthenticated", "The user is not authenticated.");
    public static readonly Error SelfFollow = new(ErrorType.Validation, "User.SelfFollow", "You cannot follow or unfollow yourself.");
    public static readonly Error AlreadyFollowing = new(ErrorType.Conflict, "User.AlreadyFollowing", "You are already following this user.");
    public static readonly Error NotFollowing = new(ErrorType.Conflict, "User.NotFollowing", "You are not following this user.");
}