using Dapper;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Comments.Queries.Common.Projections;
using SocialMedia.Application.Comments.Responses;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.Users.Responses;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Comments.Queries.GetPagedReplies;

public sealed class GetPagedRepliesQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetPagedRepliesQuery, PagedList<CommentResponse>>
{
    public async Task<Result<PagedList<CommentResponse>>> Handle(
        GetPagedRepliesQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT RepliesCount FROM CommentProjections WHERE CommentId = @ParentCommentId;

            SELECT
                CommentId AS Id,
                PostId,
                ParentCommentId,
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
            WHERE ParentCommentId = @ParentCommentId
            ORDER BY CreatedAt ASC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        using var multi = await connection.QueryMultipleAsync(
            sql,
            new
            {
                request.ParentCommentId,
                Offset = (request.Page - 1) * request.PageSize,
                request.PageSize
            });

        var totalReplies = await multi.ReadSingleOrDefaultAsync<int?>();
        if (totalReplies is null)
            return Result.Failure<PagedList<CommentResponse>>(CommentErrors.NotFound);

        var replyRows = (await multi.ReadAsync<CommentProjectionRow>()).ToList();

        var result = replyRows.Select(r => new CommentResponse
        {
            Id = r.Id,
            ParentCommentId = r.ParentCommentId,
            PostId = r.PostId,
            Content = r.Content,
            CreatedBy = new UserResponse { Id = r.UserId, Name = r.UserName, Email = r.UserEmail, AvatarUrl = r.UserAvatarUrl },
            ReactsCount = r.ReactsCount,
            RepliesCount = r.RepliesCount,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList();

        return new PagedList<CommentResponse>(totalReplies.Value, request.PageSize, request.Page, result);
    }
}
