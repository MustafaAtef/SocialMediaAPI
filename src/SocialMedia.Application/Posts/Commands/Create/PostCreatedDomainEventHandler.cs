using Dapper;

using MediatR;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Core.Events.Posts;

namespace SocialMedia.Application.Posts.Commands.Create;

public sealed class PostCreatedDomainEventHandler(ITransactionContext transactionContext)
    : INotificationHandler<PostCreatedDomainEvent>
{
    public async Task Handle(PostCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var connection = transactionContext.Connection;
        var transaction = transactionContext.Transaction;

        await connection.ExecuteAsync("""
            INSERT INTO PostProjections
                (PostId, UserId, UserName, UserEmail, UserAvatarUrl, Content, ReactsCount, CommentsCount, IsDeleted, CreatedAt)
            VALUES
                (@PostId, @UserId, @UserName, @UserEmail, @UserAvatarUrl, @Content, 0, 0, 0, @CreatedAt)
            """,
            new
            {
                notification.PostId,
                notification.UserId,
                notification.UserName,
                notification.UserEmail,
                notification.UserAvatarUrl,
                notification.Content,
                notification.CreatedAt
            }, transaction);

        if (notification.Attachments.Count == 0) return;

        await connection.ExecuteAsync("""
            INSERT INTO PostAttachmentProjections (AttachmentId, PostId, Url, AttachmentType)
            VALUES (@AttachmentId, @PostId, @Url, @AttachmentType)
            """,
            notification.Attachments.Select(a => new
            {
                a.AttachmentId,
                notification.PostId,
                a.Url,
                AttachmentType = (int)a.AttachmentType
            }), transaction);
    }
}
