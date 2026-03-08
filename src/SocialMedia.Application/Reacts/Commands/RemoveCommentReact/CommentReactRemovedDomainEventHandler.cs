using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.CommentReacts;

namespace SocialMedia.Application.Reacts.Commands.RemoveCommentReact;

public sealed class CommentReactRemovedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<CommentReactRemovedDomainEvent>
{
    public async Task Handle(CommentReactRemovedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            DELETE FROM CommentReactProjections WHERE Id = @Id;

            UPDATE CommentProjections SET ReactsCount = ReactsCount - 1 WHERE CommentId = @CommentId;
            """,
            new { notification.Id, notification.CommentId }, transaction);
    }
}
