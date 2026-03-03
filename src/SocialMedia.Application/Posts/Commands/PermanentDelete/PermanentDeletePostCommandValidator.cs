using FluentValidation;

namespace SocialMedia.Application.Posts.Commands.PermanentDelete;

public class PermanentDeletePostCommandValidator : AbstractValidator<PermanentDeletePostCommand>
{
    public PermanentDeletePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .GreaterThan(0).WithMessage("Post id must be greater than 0.");
    }
}