using System.Security.Claims;

using SocialMedia.Application.Dtos;
using SocialMedia.Core.Entities;

namespace SocialMedia.Application.UnitTests;

internal static class AuthUserTestData
{
    public static User Entity(
        int id = 1,
        string firstName = "Mostafa",
        string lastName = "Atef",
        string email = "mostafa@example.com",
        string password = "HASHED",
        bool isEmailVerified = false)
    {
        return new User
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Password = password,
            IsEmailVerified = isEmailVerified
        };
    }

    public static JwtDto Jwt(
        string token = "token",
        string refreshToken = "refresh",
        DateTime? tokenExpiry = null,
        DateTime? refreshTokenExpiry = null)
    {
        return new JwtDto
        {
            Token = token,
            RefreshToken = refreshToken,
            TokenExpirationDate = tokenExpiry ?? DateTime.UtcNow.AddMinutes(30),
            RefreshTokenExpirationDate = refreshTokenExpiry ?? DateTime.UtcNow.AddDays(7)
        };
    }

    public static ClaimsPrincipal PrincipalWithUserId(int userId, string? email = null, string? name = null, string? avatarUrl = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        if (email is not null) claims.Add(new Claim(ClaimTypes.Email, email));
        if (name is not null) claims.Add(new Claim("name", name));
        if (avatarUrl is not null) claims.Add(new Claim("avatarUrl", avatarUrl));

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new ClaimsPrincipal(identity);
    }
}
