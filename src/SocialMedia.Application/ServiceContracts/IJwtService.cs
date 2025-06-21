

using System.Security.Claims;
using SocialMedia.Application.Dtos;
using SocialMedia.Core.Entities;

namespace SocialMedia.Application.ServiceContracts;

public interface IJwtService
{
    JwtDto GenerateToken(User user);
    ClaimsPrincipal? ValidateJwt(string token);
}
