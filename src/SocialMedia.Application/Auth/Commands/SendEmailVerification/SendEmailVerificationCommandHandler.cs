using SocialMedia.Application.Common;
using SocialMedia.Core.RepositoryContracts;

using Microsoft.Extensions.Configuration;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Auth.Commands.SendEmailVerification;

public class SendEmailVerificationCommandHandler(
    IUnitOfWork unitOfWork,
    IEmailProcessorQueue emailProcessorQueue,
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
        await unitOfWork.SaveChangesAsync();

        // Refactor: raise a domain event to trigger email sending instead of directly enqueuing the email
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        var success = await emailProcessorQueue.WriteAsync(new EmailDto { User = user, Type = EmailType.Verification }, cts.Token);
        if (!success)
            return Result.Failure(AuthErrors.ServerBusy);

        return Result.Success();
    }

}
