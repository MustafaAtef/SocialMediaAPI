using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.Posts;

namespace SocialMedia.Application.Posts.Commands.PermanentDelete;

public sealed class PostPermanentDeletedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<PostPermanentDeletedDomainEvent>
{
    public async Task Handle(PostPermanentDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            DELETE FROM CommentReactProjections
            WHERE CommentId IN (SELECT CommentId FROM CommentProjections WHERE PostId = @PostId);

            DELETE FROM CommentProjections        WHERE PostId = @PostId;
            DELETE FROM PostReactProjections      WHERE PostId = @PostId;
            DELETE FROM PostAttachmentProjections WHERE PostId = @PostId;
            DELETE FROM PostProjections           WHERE PostId = @PostId;
            """,
            new { notification.PostId }, transaction);
    }
}
