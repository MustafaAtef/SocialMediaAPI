using FluentValidation;

using SocialMedia.Core.Enumerations;

namespace SocialMedia.Application.Reacts.Commands.ReactToPost;

public class ReactToPostCommandValidator : AbstractValidator<ReactToPostCommand>
{
    public ReactToPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .GreaterThan(0).WithMessage("Post id must be greater than 0.");

        RuleFor(x => x.ReactType)
            .IsInEnum().WithMessage("React type must be one of the following: Like (1), Love (2), Laugh (3), Sad (4), Angry (5), Wow (6).");
    }
}
