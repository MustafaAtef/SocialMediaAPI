using SocialMedia.Core.RepositoryContracts;

using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<VerifyEmailCommand, bool>
{
    public async Task<Result<bool>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetAsync(u => u.EmailVerificationToken == request.Token);
        if (user == null || user.EmailVerificationTokenExpiryTime < DateTime.Now)
            return Result.Failure<bool>(AuthErrors.InvalidOrExpiredToken);

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiryTime = null;
        await unitOfWork.SaveChangesAsync();

        return Result.Success(true);
    }
}
