using Dapper;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.Reacts.Queries.Common.Projections;
using SocialMedia.Application.Reacts.Queries.Common.Responses;
using SocialMedia.Application.Users.Common.Responses;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Reacts.Queries.GetPagedPostReacts;

public sealed class GetPagedPostReactsQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetPagedPostReactsQuery, PagedList<PostReactResponse>>
{
    public async Task<Result<PagedList<PostReactResponse>>> Handle(
        GetPagedPostReactsQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT ReactionsCount FROM PostProjections WHERE PostId = @PostId AND IsDeleted = 0;

            SELECT 
                Id, 
                PostId,
                UserId  AS ReactedById,
                UserName AS ReactedByName,
                UserEmail AS ReactedByEmail,
                UserAvatarUrl AS ReactedByAvatarUrl,
                ReactType,
                CreatedAt
            FROM PostReactProjections
            WHERE PostId = @PostId
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        using var multi = await connection.QueryMultipleAsync(
            sql,
            new { request.PostId, Offset = (request.Page - 1) * request.PageSize, request.PageSize });

        var totalCount = await multi.ReadSingleOrDefaultAsync<int?>();
        if (totalCount is null)
            return Result.Failure<PagedList<PostReactResponse>>(PostErrors.NotFound);

        var result = (await multi.ReadAsync<PostReactProjectionRow>()).Select(r => new PostReactResponse
        {
            Id = r.Id,
            PostId = r.PostId,
            ReactedBy = new UserResponse { Id = r.ReactedById, Name = r.ReactedByName, Email = r.ReactedByEmail, AvatarUrl = r.ReactedByAvatarUrl },
            TypeNo = r.ReactType,
            TypeName = r.ReactType.ToString(),
            CreatedAt = r.CreatedAt
        }).ToList();

        return new PagedList<PostReactResponse>(totalCount.Value, request.PageSize, request.Page, result);
    }
}
