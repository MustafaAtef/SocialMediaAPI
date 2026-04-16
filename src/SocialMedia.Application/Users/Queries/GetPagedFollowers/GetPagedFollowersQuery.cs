using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.Users.Responses;

namespace SocialMedia.Application.Users.Queries.GetPagedFollowers;

public sealed record GetPagedFollowersQuery(int UserId, int Page, int PageSize) : IQuery<PagedList<UserResponse>>;
