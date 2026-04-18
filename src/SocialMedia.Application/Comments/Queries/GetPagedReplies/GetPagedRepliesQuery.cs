using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Comments.Responses;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.Comments.Queries.GetPagedReplies;

public sealed record GetPagedRepliesQuery(int PostId, int ParentCommentId, int Page, int PageSize) : IQuery<PagedList<CommentResponse>>;
