using SocialMedia.Application.Common;
using SocialMedia.Core.RepositoryContracts;
using Microsoft.Extensions.Configuration;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Auth.Responses;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;
using SocialMedia.Core.Events.Users;
using Microsoft.Extensions.Options;
using SocialMedia.Application.Options;

namespace SocialMedia.Application.Auth.Commands.Register;

public class RegisterCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    IFileUploader fileUploader,
    IEmailOutboxWriter emailOutboxWriter,
    IOptions<ExpireDurationsOptions> expireDurationsOptions) : ICommandHandler<RegisterCommand, AuthenticatedUserResponse>
{
    private readonly ExpireDurationsOptions expireDurations = expireDurationsOptions.Value;

    public async Task<Result<AuthenticatedUserResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await unitOfWork.Users.GetAsync(u => u.Email == request.Email);
        if (existing != null)
            return Result.Failure<AuthenticatedUserResponse>(UserErrors.AlreadyExists);

        var newUser = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = passwordHasher.HashPassword(request.Password),
            IsEmailVerified = false,
        };

        JwtDto jwtData = jwtService.GenerateToken(newUser);
        newUser.RefreshToken = jwtData.RefreshToken;
        newUser.RefreshTokenExpiryTime = jwtData.RefreshTokenExpirationDate;
        newUser.EmailVerificationToken = CryptoHelper.GenerateRandomToken();
        newUser.EmailVerificationTokenExpiryTime = DateTime.Now.AddMinutes(
            expireDurations.EmailVerificationTokenExpiryMinutes);

        // REFACTOR
        if (request.Avatar != null)
        {
            var uploadedImage = await fileUploader.UploadAsync(request.Avatar, "users-avatars");
            newUser.Avatar = new Avatar
            {
                StorageProvider = uploadedImage.StorageProvider,
                Url = uploadedImage.Url
            };
        }

        unitOfWork.Users.Add(newUser);
        newUser.RaiseDomainEvent(() => new UserRegisteredDomainEvent(
            newUser.Id,
            $"{newUser.FirstName} {newUser.LastName}",
            newUser.Email,
            newUser.Avatar?.Url ?? string.Empty));

        emailOutboxWriter.QueueVerificationEmail(
            newUser.Email,
            newUser.EmailVerificationToken!,
            newUser.EmailVerificationTokenExpiryTime!.Value);
        await unitOfWork.SaveChangesAsync();

        return Result.Success(new AuthenticatedUserResponse
        {
            Id = newUser.Id,
            Name = newUser.FirstName + " " + newUser.LastName,
            Email = newUser.Email,
            IsEmailVerified = false,
            FollowersCount = newUser.FollowersCount,
            FollowingCount = newUser.FollowingCount,
            Token = jwtData.Token,
            TokenExpirationDate = jwtData.TokenExpirationDate,
            RefreshToken = jwtData.RefreshToken,
            RefreshTokenExpirationDate = jwtData.RefreshTokenExpirationDate,
            AvatarUrl = newUser.Avatar?.Url ?? ""
        });
    }

}
