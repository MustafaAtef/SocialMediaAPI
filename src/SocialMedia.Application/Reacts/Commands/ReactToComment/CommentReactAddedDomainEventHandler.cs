using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.CommentReacts;

namespace SocialMedia.Application.Reacts.Commands.ReactToComment;

public sealed class CommentReactAddedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<CommentReactAddedDomainEvent>
{
    public async Task Handle(CommentReactAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            INSERT INTO CommentReactProjections
                (Id, CommentId, UserId, UserName, UserEmail, UserAvatarUrl, ReactType, CreatedAt)
            VALUES
                (@Id, @CommentId, @UserId, @UserName, @UserEmail, @UserAvatarUrl, @ReactType, @CreatedAt);

            UPDATE CommentProjections SET ReactsCount = ReactsCount + 1 WHERE CommentId = @CommentId;
            """,
            new
            {
                notification.Id,
                notification.CommentId,
                notification.UserId,
                notification.UserName,
                notification.UserEmail,
                notification.UserAvatarUrl,
                ReactType = (int)notification.ReactType,
                notification.CreatedAt
            }, transaction);
    }
}
