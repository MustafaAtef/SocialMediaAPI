using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;

namespace SocialMedia.Application.Services;

public class UserService : IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public UserDto? GetAuthenticatedUser()
    {
        var principal = _httpContextAccessor.HttpContext.User;
        var id = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = principal?.FindFirst(ClaimTypes.Email)?.Value;
        var name = principal?.FindFirst("name")?.Value;
        var avatarUrl = principal?.FindFirst("avatarUrl")?.Value;
        if (id == null)
        {
            return null;
        }
        return new UserDto
        {
            Id = int.Parse(id),
            Email = email,
            Name = name,
            AvatarUrl = avatarUrl ?? string.Empty
        };
    }

}
