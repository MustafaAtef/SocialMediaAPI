using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.Users.Common.Responses;

namespace SocialMedia.Application.Users.Queries.GetPagedFollowing;

public sealed record GetPagedFollowingQuery(int UserId, int Page, int PageSize) : IQuery<PagedList<UserResponse>>;
