using FluentValidation.TestHelper;

using SocialMedia.Application.Posts.Commands.Create;
using SocialMedia.Application.Posts.Commands.PermanentDelete;
using SocialMedia.Application.Posts.Commands.Restore;
using SocialMedia.Application.Posts.Commands.SoftDelete;
using SocialMedia.Application.Posts.Commands.Update;

using Xunit;

namespace SocialMedia.Application.UnitTests.Posts;

[Trait("Category", "Unit")]
public class PostCommandValidatorsTests
{
    [Fact]
    public void CreatePostValidator_WhenCommandInvalid_ShouldHaveErrorForContent()
    {
        var sut = new CreatePostCommandValidator();

        var result = sut.TestValidate(new CreatePostCommand(string.Empty, new List<Microsoft.AspNetCore.Http.IFormFile>()));

        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void CreatePostValidator_WhenCommandValid_ShouldNotHaveErrorForContent()
    {
        var sut = new CreatePostCommandValidator();

        var result = sut.TestValidate(new CreatePostCommand("Post content", new List<Microsoft.AspNetCore.Http.IFormFile>()));

        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void UpdatePostValidator_WhenPostIdInvalid_ShouldHaveErrorForPostId()
    {
        var sut = new UpdatePostCommandValidator();

        var result = sut.TestValidate(new UpdatePostCommand(0, "Updated", null, null));

        result.ShouldHaveValidationErrorFor(x => x.PostId);
    }

    [Fact]
    public void UpdatePostValidator_WhenContentTooLong_ShouldHaveErrorForContent()
    {
        var sut = new UpdatePostCommandValidator();
        var longContent = new string('a', 1001);

        var result = sut.TestValidate(new UpdatePostCommand(1, longContent, null, null));

        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void UpdatePostValidator_WhenCommandValid_ShouldNotHaveErrorsForPostIdAndContent()
    {
        var sut = new UpdatePostCommandValidator();

        var result = sut.TestValidate(new UpdatePostCommand(1, "Updated", null, null));

        result.ShouldNotHaveValidationErrorFor(x => x.PostId);
        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void SoftDeletePostValidator_WhenCommandInvalid_ShouldHaveErrorForPostId()
    {
        var sut = new SoftDeletePostCommandValidator();

        var result = sut.TestValidate(new SoftDeletePostCommand(0));

        result.ShouldHaveValidationErrorFor(x => x.PostId);
    }

    [Fact]
    public void SoftDeletePostValidator_WhenCommandValid_ShouldNotHaveErrorForPostId()
    {
        var sut = new SoftDeletePostCommandValidator();

        var result = sut.TestValidate(new SoftDeletePostCommand(1));

        result.ShouldNotHaveValidationErrorFor(x => x.PostId);
    }

    [Fact]
    public void PermanentDeletePostValidator_WhenCommandInvalid_ShouldHaveErrorForPostId()
    {
        var sut = new PermanentDeletePostCommandValidator();

        var result = sut.TestValidate(new PermanentDeletePostCommand(0));

        result.ShouldHaveValidationErrorFor(x => x.PostId);
    }

    [Fact]
    public void PermanentDeletePostValidator_WhenCommandValid_ShouldNotHaveErrorForPostId()
    {
        var sut = new PermanentDeletePostCommandValidator();

        var result = sut.TestValidate(new PermanentDeletePostCommand(1));

        result.ShouldNotHaveValidationErrorFor(x => x.PostId);
    }

    [Fact]
    public void RestorePostValidator_WhenCommandInvalid_ShouldHaveErrorForPostId()
    {
        var sut = new RestorePostCommandValidator();

        var result = sut.TestValidate(new RestorePostCommand(0));

        result.ShouldHaveValidationErrorFor(x => x.PostId);
    }

    [Fact]
    public void RestorePostValidator_WhenCommandValid_ShouldNotHaveErrorForPostId()
    {
        var sut = new RestorePostCommandValidator();

        var result = sut.TestValidate(new RestorePostCommand(1));

        result.ShouldNotHaveValidationErrorFor(x => x.PostId);
    }
}
