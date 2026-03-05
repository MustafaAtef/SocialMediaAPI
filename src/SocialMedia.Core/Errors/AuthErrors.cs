namespace SocialMedia.Core.Errors;

public static class AuthErrors
{
    public static readonly Error InvalidOrExpiredToken = new("Auth.InvalidOrExpiredToken", "The token is invalid or has expired.");
    public static readonly Error InvalidRefreshToken = new("Auth.InvalidRefreshToken", "The provided refresh token is invalid or has expired.");
    public static readonly Error IncorrectOldPassword = new("Auth.IncorrectOldPassword", "The old password you entered is incorrect.");
    public static readonly Error EmailAlreadyVerified = new("Auth.EmailAlreadyVerified", "This email address has already been verified.");
    public static readonly Error ServerBusy = new("Auth.ServerBusy", "The server is currently busy, please try again later.");
}
