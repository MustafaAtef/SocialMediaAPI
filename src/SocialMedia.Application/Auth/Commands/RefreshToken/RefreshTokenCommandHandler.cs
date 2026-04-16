using System.Security.Claims;

using SocialMedia.Core.RepositoryContracts;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Auth.Responses;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IUnitOfWork unitOfWork,
    IJwtService jwtService) : ICommandHandler<RefreshTokenCommand, AuthenticatedUserResponse>
{
    public async Task<Result<AuthenticatedUserResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userClaims = jwtService.ValidateJwt(request.Token);
        if (userClaims == null)
            return Result.Failure<AuthenticatedUserResponse>(AuthErrors.InvalidRefreshToken);

        var userId = int.Parse(userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "-1");
        var user = await unitOfWork.Users.GetAsync(u => u.Id == userId, ["Avatar"]);

        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime < DateTime.Now)
            return Result.Failure<AuthenticatedUserResponse>(AuthErrors.InvalidRefreshToken);

        JwtDto jwtData = jwtService.GenerateToken(user);
        user.RefreshToken = jwtData.RefreshToken;
        user.RefreshTokenExpiryTime = jwtData.RefreshTokenExpirationDate;
        await unitOfWork.SaveChangesAsync();

        return Result.Success(new AuthenticatedUserResponse
        {
            Id = user.Id,
            Name = user.FirstName + " " + user.LastName,
            Email = user.Email,
            IsEmailVerified = user.IsEmailVerified,
            FollowersCount = user.FollowersCount,
            FollowingCount = user.FollowingCount,
            Token = jwtData.Token,
            TokenExpirationDate = jwtData.TokenExpirationDate,
            RefreshToken = jwtData.RefreshToken,
            RefreshTokenExpirationDate = jwtData.RefreshTokenExpirationDate,
            AvatarUrl = user.Avatar?.Url ?? ""
        });
    }
}
