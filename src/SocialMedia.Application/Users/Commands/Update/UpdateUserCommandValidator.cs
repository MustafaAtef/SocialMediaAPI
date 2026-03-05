using FluentValidation;

namespace SocialMedia.Application.Users.Commands.Update;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .Length(3, 50).WithMessage("First name must be between 3 and 50 characters.")
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .Length(3, 50).WithMessage("Last name must be between 3 and 50 characters.")
            .When(x => x.LastName is not null);
    }
}
