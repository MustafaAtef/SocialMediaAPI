using FluentValidation.TestHelper;

using SocialMedia.Application.Users.Commands.Follow;
using SocialMedia.Application.Users.Commands.UnFollow;
using SocialMedia.Application.Users.Commands.Update;

using Xunit;

namespace SocialMedia.Application.UnitTests;

[Trait("Category", "Unit")]
public class UserCommandValidatorsTests
{
    [Fact]
    public void FollowUserValidator_WhenCommandInvalid_ShouldHaveErrorForUserId()
    {
        var sut = new FollowUserCommandValidator();

        var result = sut.TestValidate(new FollowUserCommand(0));

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void FollowUserValidator_WhenCommandValid_ShouldNotHaveErrorForUserId()
    {
        var sut = new FollowUserCommandValidator();

        var result = sut.TestValidate(new FollowUserCommand(1));

        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void UnFollowUserValidator_WhenCommandInvalid_ShouldHaveErrorForUserId()
    {
        var sut = new UnFollowUserCommandValidator();

        var result = sut.TestValidate(new UnFollowUserCommand(0));

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void UnFollowUserValidator_WhenCommandValid_ShouldNotHaveErrorForUserId()
    {
        var sut = new UnFollowUserCommandValidator();

        var result = sut.TestValidate(new UnFollowUserCommand(1));

        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void UpdateUserValidator_WhenCommandInvalid_ShouldHaveErrorsForFirstNameAndLastName()
    {
        var sut = new UpdateUserCommandValidator();

        var result = sut.TestValidate(new UpdateUserCommand("ab", "xy", null));

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void UpdateUserValidator_WhenCommandValid_ShouldNotHaveErrorsForFirstNameAndLastName()
    {
        var sut = new UpdateUserCommandValidator();

        var result = sut.TestValidate(new UpdateUserCommand("Mostafa", "Atef", null));

        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }
}
