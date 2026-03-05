using SocialMedia.Application.Abstractions.Messaging;

namespace SocialMedia.Application.Auth.Commands.SendEmailVerification;

public record SendEmailVerificationCommand(string Email) : ICommand;
