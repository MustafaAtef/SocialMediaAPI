using FluentValidation.TestHelper;

using SocialMedia.Application.Reacts.Commands.ReactToComment;
using SocialMedia.Application.Reacts.Commands.ReactToPost;
using SocialMedia.Application.Reacts.Commands.RemoveCommentReact;
using SocialMedia.Application.Reacts.Commands.RemovePostReact;
using SocialMedia.Core.Enumerations;

using Xunit;

namespace SocialMedia.Application.UnitTests;

public class ReactCommandValidatorsTests
{
    [Fact]
    public void ReactToPostValidator_WhenCommandInvalid_ShouldHaveErrorsForPostIdAndReactType()
    {
        var sut = new ReactToPostCommandValidator();

        var result = sut.TestValidate(new ReactToPostCommand(0, (ReactType)999));

        result.ShouldHaveValidationErrorFor(x => x.PostId);
        result.ShouldHaveValidationErrorFor(x => x.ReactType);
    }

    [Fact]
    public void ReactToPostValidator_WhenCommandValid_ShouldNotHaveErrors()
    {
        var sut = new ReactToPostCommandValidator();

        var result = sut.TestValidate(new ReactToPostCommand(1, ReactType.Like));

        result.ShouldNotHaveValidationErrorFor(x => x.PostId);
        result.ShouldNotHaveValidationErrorFor(x => x.ReactType);
    }

    [Fact]
    public void ReactToCommentValidator_WhenCommandInvalid_ShouldHaveErrorsForIdsAndReactType()
    {
        var sut = new ReactToCommentCommandValidator();

        var result = sut.TestValidate(new ReactToCommentCommand(0, 0, (ReactType)999));

        result.ShouldHaveValidationErrorFor(x => x.PostId);
        result.ShouldHaveValidationErrorFor(x => x.CommentId);
        result.ShouldHaveValidationErrorFor(x => x.ReactType);
    }

    [Fact]
    public void ReactToCommentValidator_WhenCommandValid_ShouldNotHaveErrors()
    {
        var sut = new ReactToCommentCommandValidator();

        var result = sut.TestValidate(new ReactToCommentCommand(1, 1, ReactType.Love));

        result.ShouldNotHaveValidationErrorFor(x => x.PostId);
        result.ShouldNotHaveValidationErrorFor(x => x.CommentId);
        result.ShouldNotHaveValidationErrorFor(x => x.ReactType);
    }

    [Fact]
    public void RemovePostReactValidator_WhenCommandInvalid_ShouldHaveErrorForPostId()
    {
        var sut = new RemovePostReactCommandValidator();

        var result = sut.TestValidate(new RemovePostReactCommand(0));

        result.ShouldHaveValidationErrorFor(x => x.PostId);
    }

    [Fact]
    public void RemovePostReactValidator_WhenCommandValid_ShouldNotHaveErrorForPostId()
    {
        var sut = new RemovePostReactCommandValidator();

        var result = sut.TestValidate(new RemovePostReactCommand(1));

        result.ShouldNotHaveValidationErrorFor(x => x.PostId);
    }

    [Fact]
    public void RemoveCommentReactValidator_WhenCommandInvalid_ShouldHaveErrorForCommentId()
    {
        var sut = new RemoveCommentReactCommandValidator();

        var result = sut.TestValidate(new RemoveCommentReactCommand(1, 0));

        result.ShouldHaveValidationErrorFor(x => x.CommentId);
    }

    [Fact]
    public void RemoveCommentReactValidator_WhenCommandValid_ShouldNotHaveErrorForCommentId()
    {
        var sut = new RemoveCommentReactCommandValidator();

        var result = sut.TestValidate(new RemoveCommentReactCommand(1, 1));

        result.ShouldNotHaveValidationErrorFor(x => x.CommentId);
    }
}
