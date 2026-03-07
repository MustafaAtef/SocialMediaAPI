using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.Reacts.Queries.Common.Responses;

namespace SocialMedia.Application.Reacts.Queries.GetPagedPostReacts;

public sealed record GetPagedPostReactsQuery(int PostId, int Page, int PageSize) : IQuery<PagedList<PostReactResponse>>;
