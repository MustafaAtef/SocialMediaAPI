using System.Security.Claims;

using SocialMedia.Core.RepositoryContracts;

using Microsoft.AspNetCore.Http;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IUserService userService) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var authenticatedUser = userService.GetAuthenticatedUser();
        if (authenticatedUser == null)
            return Result.Failure(UserErrors.Unauthenticated);

        var user = await unitOfWork.Users.GetAsync(u => u.Id == authenticatedUser.Id);
        if (user == null)
            return Result.Failure(UserErrors.NotFound);

        if (!passwordHasher.VerifyPassword(request.OldPassword, user.Password))
            return Result.Failure(AuthErrors.IncorrectOldPassword);

        user.Password = passwordHasher.HashPassword(request.NewPassword);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
