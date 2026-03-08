using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.PostReacts;

namespace SocialMedia.Application.Reacts.Commands.RemovePostReact;

public sealed class PostReactRemovedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<PostReactRemovedDomainEvent>
{
    public async Task Handle(PostReactRemovedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            DELETE FROM PostReactProjections WHERE Id = @Id;

            UPDATE PostProjections SET ReactsCount = ReactsCount - 1 WHERE PostId = @PostId;
            """,
            new { notification.Id, notification.PostId }, transaction);
    }
}
