namespace SocialMedia.Application.Auth.Responses;

public class AuthenticatedUserResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string AvatarUrl { get; set; }
    public bool IsEmailVerified { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public string Token { get; set; }
    public DateTime TokenExpirationDate { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpirationDate { get; set; }
}
