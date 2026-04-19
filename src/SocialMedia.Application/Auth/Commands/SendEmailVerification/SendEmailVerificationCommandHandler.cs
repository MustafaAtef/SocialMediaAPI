using SocialMedia.Application.Common;
using SocialMedia.Core.RepositoryContracts;

using Microsoft.Extensions.Configuration;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;
using SocialMedia.Application.Options;
using Microsoft.Extensions.Options;

namespace SocialMedia.Application.Auth.Commands.SendEmailVerification;

public class SendEmailVerificationCommandHandler(
    IUnitOfWork unitOfWork,
    IEmailOutboxWriter emailOutboxWriter,
    IOptions<ExpireDurationsOptions> expireDurationsOptions) : ICommandHandler<SendEmailVerificationCommand>
{
    private readonly ExpireDurationsOptions expireDurations = expireDurationsOptions.Value;

    public async Task<Result> Handle(SendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        // Refactor: don't return user not found if email is not registered, to prevent email enumeration attacks
        var user = await unitOfWork.Users.GetAsync(u => u.Email == request.Email);
        if (user == null)
            return Result.Success(); // To prevent email enumeration, we return success even if the user doesn't exist.

        if (user.IsEmailVerified)
            return Result.Failure(AuthErrors.EmailAlreadyVerified);

        user.EmailVerificationToken = CryptoHelper.GenerateRandomToken();
        user.EmailVerificationTokenExpiryTime = DateTime.Now.AddMinutes(
            expireDurations.EmailVerificationTokenExpiryMinutes);

        emailOutboxWriter.QueueVerificationEmail(
            user.Email,
            user.EmailVerificationToken!,
            user.EmailVerificationTokenExpiryTime!.Value);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

}
