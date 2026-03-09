using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.Users;

namespace SocialMedia.Application.Users.Commands.Register;

public sealed class UserRegisteredDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<UserRegisteredDomainEvent>
{
    public async Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        await transactionContext.Connection.ExecuteAsync("""
            INSERT INTO UserProjections (UserId, Name, Email, AvatarUrl, FollowersCount, FollowingCount)
            VALUES (@UserId, @Name, @Email, @AvatarUrl, 0, 0)
            """,
            new
            {
                notification.UserId,
                Name = notification.UserName,
                Email = notification.UserEmail,
                AvatarUrl = notification.UserAvatarUrl
            }, transactionContext.Transaction);
    }
}
