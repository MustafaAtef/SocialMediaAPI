using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : ICommand<AuthenticatedUserDto>;
