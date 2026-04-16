using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Auth.Responses;

namespace SocialMedia.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token, string RefreshToken) : ICommand<AuthenticatedUserResponse>;
