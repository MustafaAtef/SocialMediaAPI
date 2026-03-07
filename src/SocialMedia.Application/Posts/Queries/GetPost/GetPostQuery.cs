using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Posts.Queries.Common.Responses;

namespace SocialMedia.Application.Posts.Queries.GetPost;

public sealed record GetPostQuery(int PostId) : IQuery<PostResponse>;
