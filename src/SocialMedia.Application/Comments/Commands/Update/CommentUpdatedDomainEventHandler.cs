using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.Comments;

namespace SocialMedia.Application.Comments.Commands.Update;

public sealed class CommentUpdatedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<CommentUpdatedDomainEvent>
{
    public async Task Handle(CommentUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            UPDATE CommentProjections
            SET Content = @Content, UpdatedAt = @UpdatedAt
            WHERE CommentId = @CommentId
            """,
            new { notification.CommentId, notification.Content, notification.UpdatedAt }, transaction);
    }
}
