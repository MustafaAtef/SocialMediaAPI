using FluentValidation;

namespace SocialMedia.Application.Users.Commands.UnFollow;

public class UnFollowUserCommandValidator : AbstractValidator<UnFollowUserCommand>
{
    public UnFollowUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User id must be greater than 0.");
    }
}
