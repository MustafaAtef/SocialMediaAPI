using SocialMedia.Core.RepositoryContracts;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Auth.Commands.Login;

public class LoginCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtService jwtService) : ICommandHandler<LoginCommand, AuthenticatedUserDto>
{
    public async Task<Result<AuthenticatedUserDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetAsync(u => u.Email == request.Email, ["Avatar"]);
        if (user == null || !passwordHasher.VerifyPassword(request.Password, user.Password))
            return Result.Failure<AuthenticatedUserDto>(UserErrors.InvalidCredentials);

        JwtDto jwtData = jwtService.GenerateToken(user);
        user.RefreshToken = jwtData.RefreshToken;
        user.RefreshTokenExpiryTime = jwtData.RefreshTokenExpirationDate;
        await unitOfWork.SaveChangesAsync();

        return Result.Success(new AuthenticatedUserDto
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
