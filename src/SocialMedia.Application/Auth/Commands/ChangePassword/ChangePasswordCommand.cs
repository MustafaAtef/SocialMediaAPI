using SocialMedia.Application.Abstractions.Messaging;

namespace SocialMedia.Application.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(string OldPassword, string NewPassword) : ICommand;
