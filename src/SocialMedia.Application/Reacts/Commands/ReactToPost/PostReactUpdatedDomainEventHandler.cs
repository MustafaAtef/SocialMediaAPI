using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.PostReacts;

namespace SocialMedia.Application.Reacts.Commands.ReactToPost;

public sealed class PostReactUpdatedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<PostReactUpdatedDomainEvent>
{
    public async Task Handle(PostReactUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            UPDATE PostReactProjections SET ReactType = @ReactType WHERE Id = @Id
            """,
            new { notification.Id, ReactType = (int)notification.ReactType }, transaction);
    }
}
