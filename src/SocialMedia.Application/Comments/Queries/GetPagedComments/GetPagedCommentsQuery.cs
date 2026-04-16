using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Comments.Responses;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.Comments.Queries.GetPagedComments;

public sealed record GetPagedCommentsQuery(int PostId, int Page, int PageSize, int RepliesSize) : IQuery<PagedList<CommentResponse>>;
