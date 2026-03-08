using Dapper;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.Users.Common.Responses;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Users.Queries.GetPagedFollowing;

public sealed class GetPagedFollowingQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetPagedFollowingQuery, PagedList<UserResponse>>
{
    public async Task<Result<PagedList<UserResponse>>> Handle(
        GetPagedFollowingQuery request,
        CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT FollowingCount
            FROM Users
            WHERE Id = @UserId;

            SELECT
                FollowingId AS Id,
                FollowingName AS Name,
                FollowingEmail AS Email,
                FollowingAvatarUrl AS AvatarUrl
            FROM UserFollowProjections
            WHERE FollowerId = @UserId
            ORDER BY CreatedAt DESC, FollowingId DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

        using var multi = await connection.QueryMultipleAsync(
            sql,
            new
            {
                request.UserId,
                Offset = (request.Page - 1) * request.PageSize,
                request.PageSize
            });

        var totalCount = await multi.ReadSingleOrDefaultAsync<int?>();
        if (totalCount is null)
            return Result.Failure<PagedList<UserResponse>>(UserErrors.NotFound);

        var result = (await multi.ReadAsync<UserResponse>()).ToList();

        return new PagedList<UserResponse>(totalCount.Value, request.PageSize, request.Page, result);
    }
}
