using FluentValidation;

namespace SocialMedia.Application.Comments.Commands.Delete;

public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.PostId)
            .GreaterThan(0).WithMessage("Post id must be greater than 0.");

        RuleFor(x => x.CommentId)
            .GreaterThan(0).WithMessage("Comment id must be greater than 0.");
    }
}
