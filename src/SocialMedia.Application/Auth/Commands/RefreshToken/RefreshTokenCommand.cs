using SocialMedia.Application.Abstractions.Messaging;
using SocialMedia.Application.Dtos;

namespace SocialMedia.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token, string RefreshToken) : ICommand<AuthenticatedUserDto>;
