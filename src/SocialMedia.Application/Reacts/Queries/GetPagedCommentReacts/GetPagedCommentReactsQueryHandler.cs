using Dapper;

using SocialMedia.Application.Abstractions.data;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.Reacts.Queries.Common.Projections;
using SocialMedia.Application.Reacts.Queries.Common.Responses;
using SocialMedia.Application.Users.Common.Responses;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Reacts.Queries.GetPagedCommentReacts;

public sealed class GetPagedCommentReactsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetPagedCommentReactsQuery, PagedList<CommentReactResponse>>
{
    public async Task<Result<PagedList<CommentReactResponse>>> Handle(
        GetPagedCommentReactsQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT ReactionsCount FROM CommentProjections WHERE CommentId = @CommentId;

            SELECT 
                Id, 
                CommentId,
                UserId AS ReactedById,
                UserName AS ReactedByName,
                UserEmail AS ReactedByEmail,
                UserAvatarUrl AS ReactedByAvatarUrl,
                ReactType, 
                CreatedAt
            FROM CommentReactProjections
            WHERE CommentId = @CommentId
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        using var multi = await connection.QueryMultipleAsync(
            sql,
            new { request.CommentId, Offset = (request.Page - 1) * request.PageSize, request.PageSize });

        var totalCount = await multi.ReadSingleOrDefaultAsync<int?>();
        if (totalCount is null)
            return Result.Failure<PagedList<CommentReactResponse>>(CommentErrors.NotFound);

        var result = (await multi.ReadAsync<CommentReactProjectionRow>()).Select(r => new CommentReactResponse
        {
            Id = r.Id,
            CommentId = r.CommentId,
            ReactedBy = new UserResponse { Id = r.ReactedById, Name = r.ReactedByName, Email = r.ReactedByEmail, AvatarUrl = r.ReactedByAvatarUrl },
            TypeNo = r.ReactType,
            TypeName = r.ReactType.ToString(),
            CreatedAt = r.CreatedAt
        }).ToList();

        return new PagedList<CommentReactResponse>(totalCount.Value, request.PageSize, request.Page, result);
    }
}
