using SocialMedia.Application.Common;
using SocialMedia.Core.RepositoryContracts;

using Microsoft.Extensions.Configuration;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Entities;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Auth.Commands.Register;

public class RegisterCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IJwtService jwtService,
    IFileUploader fileUploader,
    IEmailProcessorQueue emailProcessorQueue,
    IConfiguration configuration) : ICommandHandler<RegisterCommand, AuthenticatedUserDto>
{
    public async Task<Result<AuthenticatedUserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await unitOfWork.Users.GetAsync(u => u.Email == request.Email);
        if (existing != null)
            return Result.Failure<AuthenticatedUserDto>(UserErrors.AlreadyExists);

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
            configuration["EmailVerificationTokenExpiryMinutes"] != null
                ? int.Parse(configuration["EmailVerificationTokenExpiryMinutes"] ?? "")
                : 15);

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
        await unitOfWork.SaveChangesAsync();

        // REFACTOR: use domain events instead of directly enqueueing email sending task here
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        var success = await emailProcessorQueue.WriteAsync(new EmailDto { User = newUser, Type = EmailType.Verification }, cts.Token);
        if (!success)
            return Result.Failure<AuthenticatedUserDto>(AuthErrors.ServerBusy);

        return Result.Success(new AuthenticatedUserDto
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
