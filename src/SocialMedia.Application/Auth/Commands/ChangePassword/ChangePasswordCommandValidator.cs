using FluentValidation;

namespace SocialMedia.Application.Auth.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("Old password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .Length(10, 100).WithMessage("New password must be between 10 and 100 characters.")
            .NotEqual(x => x.OldPassword).WithMessage("New password must be different from the old password.");
    }
}
