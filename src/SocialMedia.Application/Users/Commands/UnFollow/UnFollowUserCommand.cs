using SocialMedia.Application.Abstractions.Messaging;

namespace SocialMedia.Application.Users.Commands.UnFollow;

public record UnFollowUserCommand(int UserId) : ICommand;
