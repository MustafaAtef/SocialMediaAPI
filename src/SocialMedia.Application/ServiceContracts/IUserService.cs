using System;
using System.Security.Claims;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.ServiceContracts;

public interface IUserService
{
    UserDto? GetAuthenticatedUser(ClaimsPrincipal? principal);
}
