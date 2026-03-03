using FluentValidation;

namespace SocialMedia.Application.Posts.Commands.Restore;

public class RestorePostCommandValidator : AbstractValidator<RestorePostCommand>
{
    public RestorePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .GreaterThan(0).WithMessage("Post id must be greater than 0.");
    }
}