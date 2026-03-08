using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.Posts;

namespace SocialMedia.Application.Posts.Commands.Restore;

public sealed class PostRestoredDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<PostRestoredDomainEvent>
{
    public async Task Handle(PostRestoredDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            UPDATE PostProjections
            SET IsDeleted = 0, DeletedAt = NULL
            WHERE PostId = @PostId
            """,
            new { notification.PostId }, transaction);
    }
}
