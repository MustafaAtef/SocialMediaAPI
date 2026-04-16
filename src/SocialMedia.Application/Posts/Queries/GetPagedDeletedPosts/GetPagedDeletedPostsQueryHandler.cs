using Dapper;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.Posts.Queries.Common.Projections;
using SocialMedia.Application.Posts.Responses;
using SocialMedia.Application.Users.Responses;
using SocialMedia.Core.Abstractions;

namespace SocialMedia.Application.Posts.Queries.GetPagedDeletedPosts;

public sealed class GetPagedDeletedPostsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetPagedDeletedPostsQuery, PagedList<PostResponse>>
{
    public async Task<Result<PagedList<PostResponse>>> Handle(
        GetPagedDeletedPostsQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT COUNT(*)
            FROM PostProjections
            WHERE UserId = @UserId AND IsDeleted = 1;

            SELECT
                p.PostId,
                p.Content,
                p.ReactsCount,
                p.CommentsCount,
                p.CreatedAt,
                p.UpdatedAt,
                p.DeletedAt,
                p.UserId,
                p.UserName,
                p.UserEmail,
                p.UserAvatarUrl,
                pa.AttachmentId,
                pa.Url AS AttachmentUrl,
                pa.AttachmentType
            FROM PostProjections p
            LEFT JOIN PostAttachmentProjections pa
                ON p.PostId = pa.PostId
            WHERE p.PostId IN (
                SELECT PostId
                FROM PostProjections
                WHERE UserId = @UserId AND IsDeleted = 1
                ORDER BY DeletedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            )
            ORDER BY p.DeletedAt DESC;
            """;

        using var multi = await connection.QueryMultipleAsync(sql, new
        {
            request.UserId,
            Offset = (request.Page - 1) * request.PageSize,
            request.PageSize
        });

        var totalCount = await multi.ReadSingleAsync<int>();
        var postDict = new Dictionary<int, PostResponse>(request.PageSize);
        var rows = await multi.ReadAsync<PostWithAttachmentProjectionRow>();

        foreach (var row in rows)
        {
            if (!postDict.TryGetValue(row.PostId, out var post))
            {
                post = new PostResponse
                {
                    Id = row.PostId,
                    Content = row.Content,
                    ReactsCount = row.ReactsCount,
                    CommentsCount = row.CommentsCount,
                    CreatedAt = row.CreatedAt,
                    UpdatedAt = row.UpdatedAt,
                    DeletedAt = row.DeletedAt,
                    Author = new UserResponse
                    {
                        Id = row.UserId,
                        Name = row.UserName,
                        Email = row.UserEmail,
                        AvatarUrl = row.UserAvatarUrl ?? string.Empty
                    },
                    Attachments = []
                };

                postDict.Add(row.PostId, post);
            }

            if (row.AttachmentId != null)
            {
                post.Attachments.Add(new AttachmentResponse
                {
                    Id = row.AttachmentId.Value,
                    Url = row.AttachmentUrl!,
                    Type = row.AttachmentType!
                });
            }
        }

        return new PagedList<PostResponse>(totalCount, request.PageSize, request.Page, postDict.Values.ToList());
    }
}
