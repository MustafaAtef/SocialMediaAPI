using FluentValidation;

namespace SocialMedia.Application.Comments.Commands.Reply;

public class ReplyCommentCommandValidator : AbstractValidator<ReplyCommentCommand>
{
    public ReplyCommentCommandValidator()
    {
        RuleFor(x => x.PostId)
            .GreaterThan(0).WithMessage("Post id must be greater than 0.");

        RuleFor(x => x.ParentCommentId)
            .GreaterThan(0).WithMessage("Parent comment id must be greater than 0.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Reply content is required.")
            .MaximumLength(500).WithMessage("Reply content max length is 500 characters.");
    }
}
