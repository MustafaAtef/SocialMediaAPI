using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Posts.Responses;

namespace SocialMedia.Application.Posts.Queries.GetPost;

public sealed record GetPostQuery(int PostId) : IQuery<PostResponse>;
