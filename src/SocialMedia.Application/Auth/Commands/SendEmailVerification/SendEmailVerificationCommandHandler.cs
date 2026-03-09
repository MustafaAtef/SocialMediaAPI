using SocialMedia.Application.Common;
using SocialMedia.Core.RepositoryContracts;

using Microsoft.Extensions.Configuration;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Auth.Commands.SendEmailVerification;

public class SendEmailVerificationCommandHandler(
    IUnitOfWork unitOfWork,
    IEmailOutboxWriter emailOutboxWriter,
    IConfiguration configuration) : ICommandHandler<SendEmailVerificationCommand>
{
    public async Task<Result> Handle(SendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        // Refactor: don't return user not found if email is not registered, to prevent email enumeration attacks
        var user = await unitOfWork.Users.GetAsync(u => u.Email == request.Email);
        if (user == null)
            return Result.Failure(UserErrors.NotFound);

        if (user.IsEmailVerified)
            return Result.Failure(AuthErrors.EmailAlreadyVerified);

        user.EmailVerificationToken = CryptoHelper.GenerateRandomToken();
        user.EmailVerificationTokenExpiryTime = DateTime.Now.AddMinutes(
            configuration["EmailVerificationTokenExpiryMinutes"] != null
                ? int.Parse(configuration["EmailVerificationTokenExpiryMinutes"] ?? "")
                : 15);

        emailOutboxWriter.QueueVerificationEmail(
            user.Email,
            user.EmailVerificationToken!,
            user.EmailVerificationTokenExpiryTime!.Value);
        await unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

}
