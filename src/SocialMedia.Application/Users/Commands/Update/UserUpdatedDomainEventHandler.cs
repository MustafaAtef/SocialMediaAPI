using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.Users;

namespace SocialMedia.Application.Users.Commands.Update;

public sealed class UserUpdatedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<UserUpdatedDomainEvent>
{
    public async Task Handle(UserUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            UPDATE UserProjections
            SET Name = @UserName, AvatarUrl = @UserAvatarUrl
            WHERE UserId = @UserId;

            UPDATE PostProjections
            SET UserName = @UserName, UserAvatarUrl = @UserAvatarUrl
            WHERE UserId = @UserId;

            UPDATE CommentProjections
            SET UserName = @UserName, UserAvatarUrl = @UserAvatarUrl
            WHERE UserId = @UserId;

            UPDATE PostReactProjections
            SET UserName = @UserName, UserAvatarUrl = @UserAvatarUrl
            WHERE UserId = @UserId;

            UPDATE CommentReactProjections
            SET UserName = @UserName, UserAvatarUrl = @UserAvatarUrl
            WHERE UserId = @UserId;

            UPDATE UserFollowProjections
            SET FollowerName = @UserName, FollowerAvatarUrl = @UserAvatarUrl
            WHERE FollowerId = @UserId;

            UPDATE UserFollowProjections
            SET FollowingName = @UserName, FollowingAvatarUrl = @UserAvatarUrl
            WHERE FollowingId = @UserId;

            UPDATE GroupMemberProjections
            SET UserName = @UserName, UserAvatarUrl = @UserAvatarUrl
            WHERE UserId = @UserId;

            UPDATE MessageProjections
            SET SenderName = @UserName, SenderAvatarUrl = @UserAvatarUrl
            WHERE FromId = @UserId;

            UPDATE MessageStatusProjections
            SET ReceiverName = @UserName, ReceiverAvatarUrl = @UserAvatarUrl
            WHERE ReceiverId = @UserId;
            """,
            new { notification.UserId, notification.UserName, notification.UserAvatarUrl }, transaction);
    }
}
