using SocialMedia.Application.Abstractions.Messaging;

namespace SocialMedia.Application.Reacts.Commands.RemoveCommentReact;

public record RemoveCommentReactCommand(int PostId, int CommentId) : ICommand;
