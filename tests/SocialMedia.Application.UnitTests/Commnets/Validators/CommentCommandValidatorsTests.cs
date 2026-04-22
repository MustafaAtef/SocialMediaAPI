using FluentValidation.TestHelper;

using SocialMedia.Application.Comments.Commands.Create;
using SocialMedia.Application.Comments.Commands.Delete;
using SocialMedia.Application.Comments.Commands.Reply;
using SocialMedia.Application.Comments.Commands.Update;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class CommentCommandValidatorsTests
{
    [Fact]
    public void CreateCommentValidator_WhenCommandInvalid_ShouldHaveErrorsForPostIdAndContent()
    {
        var sut = new CreateCommentCommandValidator();

        var result = sut.TestValidate(new CreateCommentCommand(0, string.Empty));

        result.ShouldHaveValidationErrorFor(x => x.PostId);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void CreateCommentValidator_WhenCommandValid_ShouldNotHaveErrors()
    {
        var sut = new CreateCommentCommandValidator();

        var result = sut.TestValidate(new CreateCommentCommand(1, "Great post"));

        result.ShouldNotHaveValidationErrorFor(x => x.PostId);
        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void UpdateCommentValidator_WhenCommandInvalid_ShouldHaveErrorsForIdsAndContent()
    {
        var sut = new UpdateCommentCommandValidator();

        var result = sut.TestValidate(new UpdateCommentCommand(0, 0, string.Empty));

        result.ShouldHaveValidationErrorFor(x => x.CommentId);
        result.ShouldHaveValidationErrorFor(x => x.PostId);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void UpdateCommentValidator_WhenCommandValid_ShouldNotHaveErrors()
    {
        var sut = new UpdateCommentCommandValidator();

        var result = sut.TestValidate(new UpdateCommentCommand(1, 1, "Updated content"));

        result.ShouldNotHaveValidationErrorFor(x => x.CommentId);
        result.ShouldNotHaveValidationErrorFor(x => x.PostId);
        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void DeleteCommentValidator_WhenCommandInvalid_ShouldHaveErrorsForPostIdAndCommentId()
    {
        var sut = new DeleteCommentCommandValidator();

        var result = sut.TestValidate(new DeleteCommentCommand(0, 0));

        result.ShouldHaveValidationErrorFor(x => x.PostId);
        result.ShouldHaveValidationErrorFor(x => x.CommentId);
    }

    [Fact]
    public void DeleteCommentValidator_WhenCommandValid_ShouldNotHaveErrors()
    {
        var sut = new DeleteCommentCommandValidator();

        var result = sut.TestValidate(new DeleteCommentCommand(1, 1));

        result.ShouldNotHaveValidationErrorFor(x => x.PostId);
        result.ShouldNotHaveValidationErrorFor(x => x.CommentId);
    }

    [Fact]
    public void ReplyCommentValidator_WhenCommandInvalid_ShouldHaveErrorsForIdsAndContent()
    {
        var sut = new ReplyCommentCommandValidator();

        var result = sut.TestValidate(new ReplyCommentCommand(0, 0, string.Empty));

        result.ShouldHaveValidationErrorFor(x => x.PostId);
        result.ShouldHaveValidationErrorFor(x => x.ParentCommentId);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void ReplyCommentValidator_WhenCommandValid_ShouldNotHaveErrors()
    {
        var sut = new ReplyCommentCommandValidator();

        var result = sut.TestValidate(new ReplyCommentCommand(1, 1, "Thanks for the info"));

        result.ShouldNotHaveValidationErrorFor(x => x.PostId);
        result.ShouldNotHaveValidationErrorFor(x => x.ParentCommentId);
        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }
}
