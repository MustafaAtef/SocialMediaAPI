using SocialMedia.Application.Abstractions.Messaging;

namespace SocialMedia.Application.Reacts.Commands.RemovePostReact;

public record RemovePostReactCommand(int PostId) : ICommand;
