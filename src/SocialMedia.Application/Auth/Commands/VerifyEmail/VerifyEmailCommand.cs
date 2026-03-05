using SocialMedia.Application.Abstractions.Messaging;

namespace SocialMedia.Application.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(string Token) : ICommand<bool>;
