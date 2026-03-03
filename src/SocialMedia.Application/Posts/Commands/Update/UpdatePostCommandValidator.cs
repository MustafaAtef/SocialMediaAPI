using FluentValidation;

namespace SocialMedia.Application.Posts.Commands.Update;

public class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .GreaterThan(0).WithMessage("Post id must be greater than 0.");

        RuleFor(x => x.Content)
            .MaximumLength(1000).WithMessage("Post content max length is 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Content));
    }
}