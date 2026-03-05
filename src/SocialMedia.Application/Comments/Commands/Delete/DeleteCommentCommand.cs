using SocialMedia.Application.Abstractions.Messaging;

namespace SocialMedia.Application.Comments.Commands.Delete;

public record DeleteCommentCommand(int PostId, int CommentId) : ICommand;
