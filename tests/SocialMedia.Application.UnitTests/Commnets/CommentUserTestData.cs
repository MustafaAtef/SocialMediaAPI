using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.UnitTests;

internal static class CommentUserTestData
{
    public static UserDto AuthenticatedUser(
        int id = 1,
        string name = "Mostafa",
        string email = "mostafa@example.com",
        string avatarUrl = "https://cdn/avatar.jpg")
    {
        return new UserDto
        {
            Id = id,
            Name = name,
            Email = email,
            AvatarUrl = avatarUrl
        };
    }
}
