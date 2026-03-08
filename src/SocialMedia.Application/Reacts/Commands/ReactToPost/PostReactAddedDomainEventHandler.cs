using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.PostReacts;

namespace SocialMedia.Application.Reacts.Commands.ReactToPost;

public sealed class PostReactAddedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<PostReactAddedDomainEvent>
{
    public async Task Handle(PostReactAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            INSERT INTO PostReactProjections
                (Id, PostId, UserId, UserName, UserEmail, UserAvatarUrl, ReactType, CreatedAt)
            VALUES
                (@Id, @PostId, @UserId, @UserName, @UserEmail, @UserAvatarUrl, @ReactType, @CreatedAt);

            UPDATE PostProjections SET ReactsCount = ReactsCount + 1 WHERE PostId = @PostId;
            """,
            new
            {
                notification.Id,
                notification.PostId,
                notification.UserId,
                notification.UserName,
                notification.UserEmail,
                notification.UserAvatarUrl,
                ReactType = (int)notification.ReactType,
                notification.CreatedAt
            }, transaction);
    }
}
