using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.Posts.Responses;

namespace SocialMedia.Application.Posts.Queries.GetPagedDeletedPosts;

public sealed record GetPagedDeletedPostsQuery(int Page, int PageSize) : ICurrentUserQuery<PagedList<PostResponse>>
{
    public int UserId { get; set; }
}