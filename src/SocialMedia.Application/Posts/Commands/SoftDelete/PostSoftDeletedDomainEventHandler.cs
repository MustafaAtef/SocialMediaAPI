using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.Posts;

namespace SocialMedia.Application.Posts.Commands.SoftDelete;

public sealed class PostSoftDeletedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<PostSoftDeletedDomainEvent>
{
    public async Task Handle(PostSoftDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            UPDATE PostProjections
            SET IsDeleted = 1, DeletedAt = @DeletedAt
            WHERE PostId = @PostId
            """,
            new { notification.PostId, notification.DeletedAt }, transaction);
    }
}
