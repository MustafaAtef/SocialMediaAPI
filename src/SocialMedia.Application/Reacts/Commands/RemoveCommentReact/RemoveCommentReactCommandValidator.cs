using FluentValidation;

namespace SocialMedia.Application.Reacts.Commands.RemoveCommentReact;

public class RemoveCommentReactCommandValidator : AbstractValidator<RemoveCommentReactCommand>
{
    public RemoveCommentReactCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .GreaterThan(0).WithMessage("Comment id must be greater than 0.");
    }
}
