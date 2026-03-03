using FluentValidation;

namespace SocialMedia.Application.Posts.Commands.SoftDelete;

public class SoftDeletePostCommandValidator : AbstractValidator<SoftDeletePostCommand>
{
    public SoftDeletePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .GreaterThan(0).WithMessage("Post id must be greater than 0.");
    }
}