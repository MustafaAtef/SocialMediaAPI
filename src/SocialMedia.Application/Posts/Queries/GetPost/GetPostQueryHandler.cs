using Dapper;

using SocialMedia.Application.Abstractions.Data;
using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Posts.Queries.Common.Projections;
using SocialMedia.Application.Posts.Responses;
using SocialMedia.Application.Users.Responses;
using SocialMedia.Core.Abstractions;
using SocialMedia.Core.Errors;

namespace SocialMedia.Application.Posts.Queries.GetPost;

public sealed class GetPostQueryHandler(ISqlConnectionFactory sqlConnectionFactory)
    : IQueryHandler<GetPostQuery, PostResponse>
{
    public async Task<Result<PostResponse>> Handle(GetPostQuery request, CancellationToken cancellationToken)
    {
        using var connection = sqlConnectionFactory.CreateConnection();

        const string sql = """
            SELECT 
                PostId, 
                Content, 
                ReactsCount, 
                CommentsCount, 
                CreatedAt, 
                UpdatedAt, 
                DeletedAt,
                UserId, 
                UserName, 
                UserEmail, 
                UserAvatarUrl
            FROM PostProjections
            WHERE PostId = @PostId AND IsDeleted = 0;

            SELECT 
            AttachmentId AS Id, 
            Url, 
            Type
            FROM PostAttachmentProjections
            WHERE PostId = @PostId;
            """;

        using var multi = await connection.QueryMultipleAsync(sql, new { request.PostId });
        var post = await multi.ReadSingleOrDefaultAsync<PostProjectionRow>();
        if (post is null)
            return Result.Failure<PostResponse>(PostErrors.NotFound);
        var attachments = (await multi.ReadAsync<AttachmentResponse>()).ToList();

        return new PostResponse
        {
            Id = post.PostId,
            Content = post.Content,
            Attachments = attachments,
            ReactsCount = post.ReactsCount,
            CommentsCount = post.CommentsCount,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            DeletedAt = post.DeletedAt,
            Author = new UserResponse
            {
                Id = post.UserId,
                Name = post.UserName,
                Email = post.UserEmail,
                AvatarUrl = post.UserAvatarUrl
            }
        };
    }
}
