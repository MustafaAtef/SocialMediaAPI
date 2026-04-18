namespace SocialMedia.Core.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidOrExpiredToken = new(ErrorType.Validation, "Auth.InvalidOrExpiredToken", "The token is invalid or has expired.");
    public static readonly Error InvalidRefreshToken = new(ErrorType.Validation, "Auth.InvalidRefreshToken", "The provided refresh token is invalid or has expired.");
    public static readonly Error IncorrectOldPassword = new(ErrorType.Validation, "Auth.IncorrectOldPassword", "The old password you entered is incorrect.");
    public static readonly Error EmailAlreadyVerified = new(ErrorType.Conflict, "Auth.EmailAlreadyVerified", "This email address has already been verified.");
}
