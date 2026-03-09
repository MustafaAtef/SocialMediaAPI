using SocialMedia.Application.Common;
using SocialMedia.Core.RepositoryContracts;

using Microsoft.Extensions.Configuration;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Auth.Commands.ForgetPassword;

public class ForgetPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IEmailOutboxWriter emailOutboxWriter,
    IConfiguration configuration) : ICommandHandler<ForgetPasswordCommand>
{
    public async Task<Result> Handle(ForgetPasswordCommand request, CancellationToken cancellationToken)
    {
        // REFACTOR: always return success to prevent user enumeration, but log the error for internal monitoring
        var user = await unitOfWork.Users.GetAsync(u => u.Email == request.Email);
        if (user == null)
            return Result.Failure(UserErrors.NotFound);

        user.PasswordResetToken = CryptoHelper.GenerateRandomToken();
        user.PasswordResetTokenExpiryTime = DateTime.Now.AddMinutes(
            configuration["PasswordResetTokenExpiryMinutes"] != null
                ? int.Parse(configuration["PasswordResetTokenExpiryMinutes"] ?? "")
                : 15);
        emailOutboxWriter.QueuePasswordResetEmail(
            user.Email,
            user.PasswordResetToken!,
            user.PasswordResetTokenExpiryTime!.Value);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

}
