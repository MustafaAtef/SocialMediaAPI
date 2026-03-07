using Dapper;

using SocialMedia.Application.Abstractions.data;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Comments.Queries.Common.Projections;
using SocialMedia.Application.Comments.Queries.Common.Responses;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.Users.Common.Responses;
using SocialMedia.Core.Abstractions;

namespace SocialMedia.Application.Comments.Queries.GetPagedComments;

public sealed class GetPagedCommentsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetPagedCommentsQuery, PagedList<CommentResponse>>
{
    public async Task<Result<PagedList<CommentResponse>>> Handle(
        GetPagedCommentsQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT COUNT(*)
            FROM CommentProjections
            WHERE PostId = @PostId AND ParentCommentId IS NULL;

            SELECT 
                CommentId AS Id, 
                PostId, 
                Content,
                UserId, 
                UserName, 
                UserEmail, 
                UserAvatarUrl,
                ReactsCount,
                RepliesCount,
                CreatedAt,
                UpdatedAt
            FROM CommentProjections
            WHERE PostId = @PostId AND ParentCommentId IS NULL
            ORDER BY CreatedAt ASC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        using var multi = await connection.QueryMultipleAsync(
            sql,
            new
            {
                request.PostId,
                Offset = (request.Page - 1) * request.PageSize,
                request.PageSize
            });

        var totalCount = await multi.ReadSingleAsync<int>();
        var commentRows = (await multi.ReadAsync<CommentProjectionRow>()).ToList();

        if (commentRows.Count == 0)
            return new PagedList<CommentResponse>(totalCount, request.PageSize, request.Page, []);

        var result = new List<CommentResponse>(commentRows.Count);

        foreach (var comment in commentRows)
        {
            var commentResponse = new CommentResponse
            {
                Id = comment.Id,
                PostId = comment.PostId,
                Content = comment.Content,
                CreatedBy = new UserResponse
                {
                    Id = comment.UserId,
                    Name = comment.UserName,
                    Email = comment.UserEmail,
                    AvatarUrl = comment.UserAvatarUrl
                },
                ReactsCount = comment.ReactsCount,
                RepliesCount = comment.RepliesCount,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };

            result.Add(commentResponse);
        }

        return new PagedList<CommentResponse>(totalCount, request.PageSize, request.Page, result);
    }
}
