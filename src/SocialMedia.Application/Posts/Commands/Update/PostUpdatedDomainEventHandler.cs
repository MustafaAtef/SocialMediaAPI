using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.Posts;

namespace SocialMedia.Application.Posts.Commands.Update;

public sealed class PostUpdatedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<PostUpdatedDomainEvent>
{
    public async Task Handle(PostUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            UPDATE PostProjections
            SET Content = @Content, UpdatedAt = @UpdatedAt
            WHERE PostId = @PostId
            """,
            new { notification.PostId, notification.Content, notification.UpdatedAt }, transaction);

        if (notification.RemovedAttachmentIds.Count > 0)
        {
            await connection.ExecuteAsync("""
                DELETE FROM PostAttachmentProjections WHERE AttachmentId IN @Ids
                """,
                new { Ids = notification.RemovedAttachmentIds }, transaction);
        }

        if (notification.AddedAttachments.Count > 0)
        {
            await connection.ExecuteAsync("""
                INSERT INTO PostAttachmentProjections (AttachmentId, PostId, Url, AttachmentType)
                VALUES (@AttachmentId, @PostId, @Url, @AttachmentType)
                """,
                notification.AddedAttachments.Select(a => new
                {
                    a.AttachmentId,
                    notification.PostId,
                    a.Url,
                    AttachmentType = (int)a.AttachmentType
                }), transaction);
        }
    }
}
