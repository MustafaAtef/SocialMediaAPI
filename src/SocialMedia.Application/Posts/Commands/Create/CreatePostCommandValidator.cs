using FluentValidation;

namespace SocialMedia.Application.Posts.Commands.Create;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Post content is required.")
            .MaximumLength(1000).WithMessage("Post content max length is 1000 characters.");
    }
}