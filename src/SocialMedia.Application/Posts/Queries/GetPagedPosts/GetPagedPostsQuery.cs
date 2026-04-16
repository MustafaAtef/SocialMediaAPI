using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.Posts.Responses;

namespace SocialMedia.Application.Posts.Queries.GetPagedPosts;

public sealed record GetPagedPostsQuery(int UserId, int Page, int PageSize) : IQuery<PagedList<PostResponse>>;
