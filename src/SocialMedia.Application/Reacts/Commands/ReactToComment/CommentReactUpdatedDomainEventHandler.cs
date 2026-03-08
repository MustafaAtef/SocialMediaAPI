using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.CommentReacts;

namespace SocialMedia.Application.Reacts.Commands.ReactToComment;

public sealed class CommentReactUpdatedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<CommentReactUpdatedDomainEvent>
{
    public async Task Handle(CommentReactUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            UPDATE CommentReactProjections SET ReactType = @ReactType WHERE Id = @Id
            """,
            new { notification.Id, ReactType = (int)notification.ReactType }, transaction);
    }
}
