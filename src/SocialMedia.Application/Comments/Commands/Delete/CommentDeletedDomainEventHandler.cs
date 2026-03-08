using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.Comments;

namespace SocialMedia.Application.Comments.Commands.Delete;

public sealed class CommentDeletedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<CommentDeletedDomainEvent>
{
    public async Task Handle(CommentDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            UPDATE cpParent
            SET cpParent.RepliesCount = cpParent.RepliesCount - 1
            FROM CommentProjections cpParent
            INNER JOIN CommentProjections cpChild
                ON cpParent.CommentId = cpChild.ParentCommentId
            WHERE cpChild.CommentId = @CommentId
              AND cpChild.ParentCommentId IS NOT NULL;

            DELETE FROM CommentReactProjections
            WHERE CommentId = @CommentId OR CommentId IN (
                SELECT CommentId FROM CommentProjections WHERE ParentCommentId = @CommentId
            );

            DELETE FROM CommentProjections WHERE ParentCommentId = @CommentId;
            DELETE FROM CommentProjections WHERE CommentId = @CommentId;

            UPDATE PostProjections
            SET CommentsCount = CommentsCount - (1 + @RepliesCount)
            WHERE PostId = @PostId;
            """,
            new { notification.CommentId, notification.RepliesCount, notification.PostId }, transaction);
    }
}
