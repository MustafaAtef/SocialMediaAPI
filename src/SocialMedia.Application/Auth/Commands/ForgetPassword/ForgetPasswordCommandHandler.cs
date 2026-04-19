using SocialMedia.Application.Common;
using SocialMedia.Core.RepositoryContracts;

using Microsoft.Extensions.Configuration;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;
using Microsoft.Extensions.Options;
using SocialMedia.Application.Options;

namespace SocialMedia.Application.Auth.Commands.ForgetPassword;

public class ForgetPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IEmailOutboxWriter emailOutboxWriter,
    IOptions<ExpireDurationsOptions> expireDurationsOptions) : ICommandHandler<ForgetPasswordCommand>
{
    private readonly ExpireDurationsOptions expireDurations = expireDurationsOptions.Value;
    public async Task<Result> Handle(ForgetPasswordCommand request, CancellationToken cancellationToken)
    {

        var user = await unitOfWork.Users.GetAsync(u => u.Email == request.Email);
        if (user == null)
            return Result.Success(); // To prevent email enumeration, we return success even if the user doesn't exist.

        user.PasswordResetToken = CryptoHelper.GenerateRandomToken();
        user.PasswordResetTokenExpiryTime = DateTime.Now.AddMinutes(
            expireDurations.PasswordResetTokenExpiryMinutes);
        emailOutboxWriter.QueuePasswordResetEmail(
            user.Email,
            user.PasswordResetToken!,
            user.PasswordResetTokenExpiryTime!.Value);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

}
