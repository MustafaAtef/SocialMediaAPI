using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Comments.Responses;

namespace SocialMedia.Application.Comments.Commands.Create;

public record CreateCommentCommand(int PostId, string Content) : ICommand<CommentResponse>;
