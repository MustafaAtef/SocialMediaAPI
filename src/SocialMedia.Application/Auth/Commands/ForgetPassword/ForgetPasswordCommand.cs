using SocialMedia.Application.Abstractions.Messaging;

namespace SocialMedia.Application.Auth.Commands.ForgetPassword;

public record ForgetPasswordCommand(string Email) : ICommand;
