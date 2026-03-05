using SocialMedia.Application.Common;
using SocialMedia.Core.RepositoryContracts;

using Microsoft.Extensions.Configuration;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.ServiceContracts;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Auth.Commands.ForgetPassword;

public class ForgetPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IEmailProcessorQueue emailProcessorQueue,
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
        await unitOfWork.SaveChangesAsync();
        // REFACTOR: use a domain event to trigger the email sending instead of directly writing to the queue here
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        var success = await emailProcessorQueue.WriteAsync(new EmailDto { User = user, Type = EmailType.ForgetPassword }, cts.Token);
        if (!success)
            return Result.Failure(AuthErrors.ServerBusy);

        return Result.Success();
    }

}
