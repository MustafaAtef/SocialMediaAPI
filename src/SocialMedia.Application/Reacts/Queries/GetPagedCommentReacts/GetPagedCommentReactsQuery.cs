using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;
using SocialMedia.Application.Reacts.Responses;

namespace SocialMedia.Application.Reacts.Queries.GetPagedCommentReacts;

public sealed record GetPagedCommentReactsQuery(int CommentId, int Page, int PageSize) : IQuery<PagedList<CommentReactResponse>>;
