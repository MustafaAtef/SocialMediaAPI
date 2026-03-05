using FluentValidation;

namespace SocialMedia.Application.Reacts.Commands.RemovePostReact;

public class RemovePostReactCommandValidator : AbstractValidator<RemovePostReactCommand>
{
    public RemovePostReactCommandValidator()
    {
        RuleFor(x => x.PostId)
            .GreaterThan(0).WithMessage("Post id must be greater than 0.");
    }
}
