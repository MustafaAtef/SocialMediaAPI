using SocialMedia.Application.Abstractions.Messaging;

namespace SocialMedia.Application.Users.Commands.Follow;

public record FollowUserCommand(int UserId) : ICommand;
