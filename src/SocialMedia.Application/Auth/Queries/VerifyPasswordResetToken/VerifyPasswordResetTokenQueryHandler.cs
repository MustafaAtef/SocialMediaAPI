using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;
using SocialMedia.Core.RepositoryContracts;

namespace SocialMedia.Application.Auth.Queries.VerifyPasswordResetToken;

public sealed class VerifyPasswordResetTokenQueryHandler(IUnitOfWork unitOfWork)
    : IQueryHandler<VerifyPasswordResetTokenQuery, bool>
{
    public async Task<Result<bool>> Handle(
        VerifyPasswordResetTokenQuery request,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetAsync(u => u.PasswordResetToken == request.Token);
        if (user == null || user.PasswordResetTokenExpiryTime < DateTime.Now)
            return Result.Failure<bool>(AuthErrors.InvalidOrExpiredToken);

        return Result.Success(true);
    }
}