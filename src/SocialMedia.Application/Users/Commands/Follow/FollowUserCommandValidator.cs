using FluentValidation;

namespace SocialMedia.Application.Users.Commands.Follow;

public class FollowUserCommandValidator : AbstractValidator<FollowUserCommand>
{
    public FollowUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User id must be greater than 0.");
    }
}
