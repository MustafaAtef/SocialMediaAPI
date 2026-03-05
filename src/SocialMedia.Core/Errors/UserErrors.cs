
namespace SocialMedia.Core.Errors;

public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "The user was not found.");

    public static readonly Error AlreadyExists = new("User.AlreadyExists", "A user with the same email already exists.");

    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "The provided credentials are invalid.");

    public static readonly Error Unauthenticated = new("User.Unauthenticated", "The user is not authenticated.");
    public static readonly Error SelfFollow = new("User.SelfFollow", "You cannot follow or unfollow yourself.");
    public static readonly Error AlreadyFollowing = new("User.AlreadyFollowing", "You are already following this user.");
    public static readonly Error NotFollowing = new("User.NotFollowing", "You are not following this user.");
}