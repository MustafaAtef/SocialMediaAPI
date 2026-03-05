using SocialMedia.Application.Abstractions.Messaging;

namespace SocialMedia.Application.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(string Token, string NewPassword) : ICommand;
