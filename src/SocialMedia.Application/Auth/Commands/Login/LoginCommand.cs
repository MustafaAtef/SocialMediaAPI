using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Auth.Responses;

namespace SocialMedia.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : ICommand<AuthenticatedUserResponse>;
