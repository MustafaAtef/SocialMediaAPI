using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Comments.Queries.Common.Responses;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.Comments.Queries.GetPagedReplies;

public sealed record GetPagedRepliesQuery(int ParentCommentId, int Page, int PageSize) : IQuery<PagedList<RepliesResponse>>;
