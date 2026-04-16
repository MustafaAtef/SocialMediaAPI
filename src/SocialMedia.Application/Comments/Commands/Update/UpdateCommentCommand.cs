using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Comments.Responses;

namespace SocialMedia.Application.Comments.Commands.Update;

public record UpdateCommentCommand(int CommentId, int PostId, string Content) : ICommand<CommentResponse>;
