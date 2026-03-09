using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.UserFollows;

namespace SocialMedia.Application.Users.Commands.Follow;

public sealed class UserFollowedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<UserFollowedDomainEvent>
{
    public async Task Handle(UserFollowedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            INSERT INTO UserFollowProjections
                (FollowerId, FollowingId, FollowerName, FollowerEmail, FollowerAvatarUrl,
                 FollowingName, FollowingEmail, FollowingAvatarUrl, CreatedAt)
            VALUES
                (@FollowerId, @FollowingId, @FollowerName, @FollowerEmail, @FollowerAvatarUrl,
                 @FollowingName, @FollowingEmail, @FollowingAvatarUrl, @CreatedAt);

            UPDATE UserProjections SET FollowingCount = FollowingCount + 1 WHERE UserId = @FollowerId;
            UPDATE UserProjections SET FollowersCount = FollowersCount + 1 WHERE UserId = @FollowingId;
            """,
            new
            {
                notification.FollowerId,
                notification.FollowingId,
                notification.FollowerName,
                notification.FollowerEmail,
                notification.FollowerAvatarUrl,
                notification.FollowingName,
                notification.FollowingEmail,
                notification.FollowingAvatarUrl,
                notification.CreatedAt
            }, transaction);
    }
}
