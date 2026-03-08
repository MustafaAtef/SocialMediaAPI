using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.UserFollows;

namespace SocialMedia.Application.Users.Commands.UnFollow;

public sealed class UserUnfollowedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<UserUnfollowedDomainEvent>
{
    public async Task Handle(UserUnfollowedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            DELETE FROM UserFollowProjections
            WHERE FollowerId = @FollowerId AND FollowingId = @FollowingId
            """,
            new { notification.FollowerId, notification.FollowingId }, transaction);
    }
}
