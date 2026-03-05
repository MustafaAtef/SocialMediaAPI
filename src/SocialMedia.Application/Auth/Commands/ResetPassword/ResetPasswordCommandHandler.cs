using SocialMedia.Core.RepositoryContracts;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetAsync(u => u.PasswordResetToken == request.Token);
        if (user == null || user.PasswordResetTokenExpiryTime < DateTime.Now)
            return Result.Failure(AuthErrors.InvalidOrExpiredToken);

        user.Password = passwordHasher.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiryTime = null;
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
