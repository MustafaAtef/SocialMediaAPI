using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.Comments;

namespace SocialMedia.Application.Comments.Commands.Create;

public sealed class CommentCreatedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<CommentCreatedDomainEvent>
{
    public async Task Handle(CommentCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            INSERT INTO CommentProjections
                (CommentId, PostId, ParentCommentId, UserId, UserName, UserEmail, UserAvatarUrl, Content, ReactsCount, RepliesCount, CreatedAt)
            VALUES
                (@CommentId, @PostId, @ParentCommentId, @UserId, @UserName, @UserEmail, @UserAvatarUrl, @Content, 0, 0, @CreatedAt);

            UPDATE PostProjections
            SET CommentsCount = CommentsCount + 1
            WHERE PostId = @PostId;

            UPDATE CommentProjections
            SET RepliesCount = RepliesCount + 1
            WHERE CommentId = @ParentCommentId AND @ParentCommentId IS NOT NULL;
            """,
            new
            {
                notification.CommentId,
                notification.PostId,
                notification.ParentCommentId,
                notification.UserId,
                notification.UserName,
                notification.UserEmail,
                notification.UserAvatarUrl,
                notification.Content,
                notification.CreatedAt
            }, transaction);
    }
}
